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
                await StartRSVP(context);
            }
            else
            {
                await context.PostAsync("Ok! Let me know when you're ready. Please respond before May 15th.");
                context.Call<RSVPMainMenu>(new FormDialog<RSVPMainMenu>(new RSVPMainMenu()), OnMainMenu);
            }
        }


        public async Task StartRSVP(IDialogContext context)
        {
            var hydratedPerson = new Person();
            if (user_context != null)
            {
                hydratedPerson.Name = user_context.Name;
                hydratedPerson.CellPhone = user_context.PhoneNumber;
            }
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
                await context.PostAsync("You will be missed but thank you for letting us know.");
                context.Done<object>(null);
            }
        }
        public async Task OnDinnerOption(IDialogContext context, IAwaitable<DinnerOption> dinnerOption)
        {
            dinner_option = await dinnerOption;
            await context.PostAsync(String.Format("Thanks {0} we'll have that {1} ready for you.", person.Name, dinner_option.PlateOption.ToString()));
            context.Call<RSVPMainMenu>(new FormDialog<RSVPMainMenu>(new RSVPMainMenu(), options: FormOptions.PromptInStart), OnMainMenu);
        }
    }
}