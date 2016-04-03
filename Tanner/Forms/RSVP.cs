﻿/*
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
        [Prompt("What's your full name? {?(Say ok if it's {Name})}")]
        public string Name;

        [Prompt("What is your cellphone number? {?(Say ok if it's {CellPhone})}")]
        public string CellPhone;
 
        [Prompt("Alright {Name}, will you be attending our wedding on June 4th at Golden Gardens in Seattle?")]
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

        public static IForm<DinnerOption> BuildForm()
        {
            return new FormBuilder<DinnerOption>().Build();
        }
    }
}