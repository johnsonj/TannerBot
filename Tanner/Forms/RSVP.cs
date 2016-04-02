/*
 * TANNERBOT
 *
 * RSVP Forms
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;


namespace Tanner.Forms
{
    [Serializable]
    [Template(TemplateUsage.NotUnderstood, 
            "Sorry, I do not understand \"{0}\".", 
            "Try again, I don't get \"{0}\".",
            "{0}? That doesn't make sense to me, try again",
            "Sorry, I'm just a bot, I don't understand \"{0}\", blame Jeff and try again")]
    public class Person
    {
        [Prompt("What is your full name?")]
        public string Name;

        [Prompt("What is your cellphone number?")]
        public string CellPhone;

        // TODO: Address
 
        [Prompt("Will you be attending {Name}? {||}")]
        public bool Attendance;
        
        public static IForm<Person> BuildForm()
        {
            return new FormBuilder<Person>().Build();
        }
    }

    public enum PlateOptions
    {
        TomattoWellington /* vegitarian */,
        FlankSteak,
        Salmon,
        TacosFromDrewsDirtyTacoTruck,
    };

    // TODO: Friendly responses
    [Serializable]
    public class DinnerOption
    {
        [Prompt("What would you like to eat? {||}")]
        public PlateOptions? PlateOption;

        private Person person;

        public static IForm<DinnerOption> BuildForm()
        {
            return new FormBuilder<DinnerOption>().Build();
        }

        public DinnerOption(Person person)
        {
            this.person = person;
        }

        public Person GetPerson()
        {
            return person;
        }
    }
}