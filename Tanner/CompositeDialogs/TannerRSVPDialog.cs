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
    [Serializable]
    class TannerRSVPDialog : IDialog
    {
        private UserContext user_context;
        private Person person;
        private DinnerOption dinner_option;
        private bool in_guest_rsvp = false;

        public TannerRSVPDialog(UserContext context)
        {
            this.user_context = context;
        }


        [Serializable]
        public class RSVPMainMenu
        {
            [Prompt("Are you ready to RSVP today?")]
            public bool? rsvp;
        }

        async Task IDialog.StartAsync(IDialogContext context)
        {
            context.Call<RSVPMainMenu>(new FormDialog<RSVPMainMenu>(new RSVPMainMenu()), OnMainMenu);
        }

        public async Task OnMainMenu(IDialogContext context, IAwaitable<RSVPMainMenu> mainMenu)
        {
            if ((await mainMenu).rsvp.Value)
            {
                var hydratedPerson = new Person();
                if (user_context.MainRSVP != null && user_context.MainRSVP.person != null)
                    hydratedPerson = user_context.MainRSVP.person;

                await StartRSVP(context, hydratedPerson);
            }
            else
            {
                await context.PostAsync("Ok! Let me know when you're ready. Please respond before May 15th.");
                context.Call<RSVPMainMenu>(new FormDialog<RSVPMainMenu>(new RSVPMainMenu()), OnMainMenu);
            }
        }


        public async Task StartRSVP(IDialogContext context, Person hydratedPerson)
        {
            // TODO: Tell the user here if they do or do not have an RSVP
            context.Call<Person>(new FormDialog<Person>(hydratedPerson, options: FormOptions.PromptInStart), OnPerson);
        }

        public async Task OnPerson(IDialogContext context, IAwaitable<Person> personTask)
        {
            person = await personTask;
            if (person.Attendance)
            {
                context.Call<DinnerOption>(new FormDialog<DinnerOption>(new DinnerOption(), options: FormOptions.PromptInStart), OnDinnerOption);
            }
            else
            {
                if(in_guest_rsvp)
                {
                    if (user_context.GuestRSVP == null)
                        user_context.GuestRSVP = new SingleRSVP();

                    user_context.GuestRSVP.is_coming = false;
                    await context.PostAsync("Thanks for confirming!");
                    await UserContextFactory.EnsurePresisted(user_context);
                    await OnAllDone(context);
                }
                else
                {
                    // REVIEW: Do we need to hydrate MainRSVP.person?
                    if (user_context.MainRSVP == null)
                        user_context.MainRSVP = new SingleRSVP();

                    user_context.MainRSVP.is_coming = false;
                    await UserContextFactory.EnsurePresisted(user_context);
                    await context.PostAsync("You will be missed, thank you for letting us know.");
                    context.Done<object>(null);
                }
            }
        }
        public async Task OnDinnerOption(IDialogContext context, IAwaitable<DinnerOption> dinnerOption)
        {
            dinner_option = await dinnerOption;

            user_context.Name = person.Name;
            user_context.PhoneNumber = person.CellPhone;

            var current_rsvp = new SingleRSVP();
            current_rsvp.is_coming = true;
            current_rsvp.dinner_option = dinner_option;
            current_rsvp.person = person;

            if(!in_guest_rsvp)
                user_context.MainRSVP = current_rsvp;
            else
                user_context.GuestRSVP = current_rsvp;
            
            await UserContextFactory.EnsurePresisted(user_context);

            // Decide what to do next
            if (!in_guest_rsvp && user_context.GuestAllotted)
            {
                in_guest_rsvp = true;
                var hydratedPerson = new Person();
                if (user_context.GuestRSVP != null)
                {
                    hydratedPerson = user_context.GuestRSVP.person;
                }
                await StartRSVP(context, hydratedPerson);
            }
            else
            {
                await OnAllDone(context);
            }

        }
        public async Task OnAllDone(IDialogContext context)
        {
            // TODO: Nice confirmation here
            await context.PostAsync(String.Format("Thanks {0} we'll have that {1} ready for you.", person.Name));
            context.Call<RSVPMainMenu>(new FormDialog<RSVPMainMenu>(new RSVPMainMenu(), options: FormOptions.PromptInStart), OnMainMenu);
        }
    }
}