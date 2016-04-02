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

namespace Tanner.CompositeDialogs
{
    [Serializable]
    class TannerRSVPDialog : IDialog
    {
        async Task IDialog.StartAsync(IDialogContext context)
        {
            context.Call<Person>(new FormDialog<Person>(new Person()), AfterPerson);
        }

        public async Task AfterPerson(IDialogContext context, IAwaitable<Person> personTask)
        {
            var person = await personTask;
            if (person.Attendance)
            {
                context.Call<DinnerOption>(new FormDialog<DinnerOption>(new DinnerOption(person), options: FormOptions.PromptInStart), AfterDinnerOption);
            }
            else
            {
                await context.PostAsync("You will be missed but thank you for letting us know!");
            }
        }
        public async Task AfterDinnerOption(IDialogContext context, IAwaitable<DinnerOption> dinnerOption)
        {
            var result = await dinnerOption;
            // TODO: builder somewhere else
            var rsvp = new Persistence.SingleRSVP();
            rsvp.dinner_option = result;
            rsvp.person = result.GetPerson();
            await context.PostAsync(String.Format("Thanks {0} we'll have that {1} ready for you.", rsvp.person.Name, rsvp.dinner_option.PlateOption.ToString()));
        }
    }
}