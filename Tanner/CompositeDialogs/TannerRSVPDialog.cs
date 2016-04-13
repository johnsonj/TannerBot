/*
 * TANNERBOT
 *
 * TannerRSVPDialog: Dialog to handle the RSVP workflow
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Dialogs;
using Tanner.Forms;
using System.Threading.Tasks;
using Tanner.Persistence;

namespace Tanner.CompositeDialogs
{
    //public enum RSVPMenuOption
    //{
    //   [Terms("yes", "sure", "ok", "rsvp", "go")]
    //    Yes,
    //   [Terms("no", "not today", "no thanks", "nope")]
    //    No,
    //}

    [Serializable]
    public class RSVPMainMenu
    {
        [Prompt("{RSVPPrompt}")]
        public bool? rsvp;

        public string RSVPPrompt
        {
            get
            {
                if (m_rsvp_menu != null)
                    return m_rsvp_menu;
                else
                    return "Are you ready to RSVP today?";
            }
            set { m_rsvp_menu = value; }
        }

        private string m_rsvp_menu;
    }

    [Serializable]
    public class SingleGuestRSVP
    {
        [Prompt("Will you be bringing a guest?")]
        public bool? guest;
    }

    [Serializable]
    class TannerRSVPDialog2 : IDialog<object>
    {
        private UserContext m_userContext;
        private SinglePersonRSVP m_currentPerson;
        private bool m_fInGuestRsvp = false;

        public TannerRSVPDialog2(UserContext context)
        {
            this.m_userContext = context;
        }

        async Task IDialog<object>.StartAsync(IDialogContext context)
        {
            var menu = new RSVPMainMenu();

            if (m_userContext.MainRSVP != null)
            {
                await context.PostAsync("I have your RSVP as: " + DescribeFullRSVP(m_userContext) + ". Thank you!");
                menu.RSVPPrompt = "Do you need to change your RSVP? I have it as " + DescribeFullRSVP(m_userContext);
                context.Call<RSVPMainMenu>(new FormDialog<RSVPMainMenu>(menu, options: FormOptions.None), OnMainMenu);
            }
            else
            {
                menu.RSVPPrompt = "Are you ready to RSVP today?";
                context.Call<RSVPMainMenu>(new FormDialog<RSVPMainMenu>(menu), OnMainMenu);
            }
        }

        public async Task OnMainMenu(IDialogContext context, IAwaitable<RSVPMainMenu> mainMenu)
        {
            if ((await mainMenu).rsvp.Value)
            {
                // Reset all of the guest state
                m_fInGuestRsvp = false;
                m_userContext.MainRSVP = null;
                m_userContext.GuestRSVP = null;

                var hydratedPerson = new SinglePersonRSVP();
                hydratedPerson.FullNamePrompt = "your";
                if (m_userContext.Name != null)
                {
                    hydratedPerson.FullName = m_userContext.Name;
                    hydratedPerson.CellPhoneNumber = m_userContext.PhoneNumber;
                }

                StartRSVP(context, hydratedPerson);
            }
            else
            {
                if (this.m_userContext.MainRSVP == null)
                {
                    // No RSVP exists yet
                    await context.PostAsync("Ok! Let me know when you're ready. Please respond before May 15th.");
                    context.Call<RSVPMainMenu>(new FormDialog<RSVPMainMenu>(new RSVPMainMenu()), OnMainMenu);
                }
                else
                {
                    await OnAllDone(context);
                }

            }
        }


        public void StartRSVP(IDialogContext context, SinglePersonRSVP hydratedPerson)
        {
            context.Call(new FormDialog<SinglePersonRSVP>(hydratedPerson, options: FormOptions.PromptInStart), OnPerson);
        }

        public void StartGuestRSVP(IDialogContext context)
        {
            m_fInGuestRsvp = true;
            context.Call(new FormDialog<SingleGuestRSVP>(new SingleGuestRSVP(), options: FormOptions.PromptInStart), OnSingleGuestRSVP);
        }

        public async Task OnSingleGuestRSVP(IDialogContext context, IAwaitable<SingleGuestRSVP> singleGuestRSVPTask)
        {
            var guestRSVP = await singleGuestRSVPTask;
            if (m_userContext.GuestRSVP == null)
            {
                m_userContext.GuestRSVP = new SingleRSVP();
                m_userContext.GuestRSVP.Person = new SinglePersonRSVP();
                m_userContext.GuestRSVP.Person.CellPhoneNumber = "(optional)";
            }

            m_currentPerson = m_userContext.GuestRSVP.Person;
            m_currentPerson.FullNamePrompt = "your guest's";

            if (guestRSVP.guest.Value)
            {
                m_userContext.GuestRSVP.Person.Attendance = true;
                StartRSVP(context, m_currentPerson);
            }
            else
            {
                m_userContext.GuestRSVP.Person.Attendance = false;
                await context.PostAsync("Sorry they can't make it, but thanks for confirming.");
                await UserContextFactory.EnsurePresisted(m_userContext);
                await OnAllDone(context);
            }
        }

        public async Task OnPerson(IDialogContext context, IAwaitable<SinglePersonRSVP> personTask)
        {
            m_currentPerson = await personTask;
            if (m_currentPerson.Attendance.Value)
            {
                var dinnerOption = new DinnerOption();
                dinnerOption.GuestName = m_fInGuestRsvp ? m_currentPerson.FullName : "you";
                context.Call<DinnerOption>(new FormDialog<DinnerOption>(dinnerOption, options: FormOptions.PromptInStart), OnDinnerOption);
            }
            else
            {
                if(m_fInGuestRsvp)
                {
                    // REVIEW: should this be reached?
                    throw new System.InvalidOperationException();
                }
                else
                {
                    if (m_userContext.MainRSVP == null)
                    {
                        m_userContext.MainRSVP = new SingleRSVP();
                        m_userContext.MainRSVP.Person = new SinglePersonRSVP();
                        m_userContext.MainRSVP.Person.FullName = m_userContext.Name;
                    }

                    m_userContext.MainRSVP.Person.Attendance = false;
                    await UserContextFactory.EnsurePresisted(m_userContext);
                    await context.PostAsync("You will be missed, thank you for letting us know.");
                    context.Done<object>(null);
                }
            }
        }
        public async Task OnDinnerOption(IDialogContext context, IAwaitable<DinnerOption> dinnerOptionTask)
        {
            var dinnerOption = await dinnerOptionTask;

            if(!m_fInGuestRsvp)
            {
                // Ensure the main context object is up to date.
                // This is redundant and probably should be removed
                m_userContext.Name = m_currentPerson.FullName;
                m_userContext.PhoneNumber = m_currentPerson.CellPhoneNumber;
            }

            var current_rsvp = new SingleRSVP();
            current_rsvp.DinnerOption = dinnerOption;
            current_rsvp.Person = m_currentPerson;
            // REVIEW: needed?
            current_rsvp.Person.Attendance = true;

            if(!m_fInGuestRsvp)
                m_userContext.MainRSVP = current_rsvp;
            else
                m_userContext.GuestRSVP = current_rsvp;
            
            await UserContextFactory.EnsurePresisted(m_userContext);

            // Decide what to do next
            if (!m_fInGuestRsvp && m_userContext.GuestAllotted)
            {
                StartGuestRSVP(context);
            }
            else
            {
                m_fInGuestRsvp = false;
                await OnAllDone(context);
            }

        }
        public async Task OnAllDone(IDialogContext context)
        {
            await context.PostAsync(String.Format("Thanks for your RSVP!"));
            context.Done<object>(null);
        }

        public string DescribeFullRSVP(UserContext context)
        {
            if (context == null)
                throw new System.InvalidOperationException();

            string res = DescribeSingleRSVP(context.MainRSVP, true);

            if (context.MainRSVP != null)
            {
                if (context.GuestRSVP != null && context.GuestRSVP.Person != null && context.GuestRSVP.Person.Attendance.Value)
                {
                    res += " with guest " + DescribeSingleRSVP(context.GuestRSVP, false);
                }
                else
                {
                    res += " with no guest";
                }
            }

            return res;
        }

        public string DescribeSingleRSVP(SingleRSVP rsvp, bool show_attendance)
        {
            string res = "";

            if(rsvp == null)
            {
                res = "Not yet responded to invitation";
            }
            else
            {
                res += rsvp.Person.FullName;
                if(rsvp.Person.Attendance.Value)
                {
                    if (show_attendance)
                        res += " is attending and eating ";
                    else
                        res += " who is eating ";

                    res += rsvp.DinnerOption.PlateOption.ToString();
                }
                else
                {
                    res += " is not attending";
                }
            }

            return res;
        }
    }
}