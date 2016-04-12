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
    public class SinglePersonRSVP
    {
        public string FullName;

        public string CellPhoneNumber;
 
        [Prompt("Will {FullName} be attending the Johnson-Klimczak wedding on June 4th at Golden Gardens in Seattle?")]
        public bool? Attendance;
        
        public static IForm<SinglePersonRSVP> BuildForm()
        {
            return new FormBuilder<SinglePersonRSVP>().Build();
        }
    }

    public enum PlateOptions
    {
        TomatoWellington /* vegitarian */,
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