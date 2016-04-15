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
using System.ComponentModel;
using System.Runtime.Serialization;

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
        [Prompt("What is {FullNamePrompt} full name?")]
        public string FullName;

        public string FullNamePrompt;

        public string CellPhoneNumber;
 
        [Prompt("Will {FullName} be attending the Johnson-Klimczak wedding on June 4th at Golden Gardens in Seattle?")]
        public bool? Attendance;
        
        public static IForm<SinglePersonRSVP> BuildForm()
        {
            return new FormBuilder<SinglePersonRSVP>()
                .Field(nameof(SinglePersonRSVP.FullName))
                .Field(nameof(SinglePersonRSVP.CellPhoneNumber))
                .Field(nameof(SinglePersonRSVP.Attendance))
                .Build();
        }
    }

    [Serializable]
    public enum PlateOptions
    {
        [EnumMember(Value = "Tomato Wellington")]
        [Describe("Tomato Wellington (Vegetarian)")]
        TomatoWellington,
        [EnumMember(Value = "Flank Steak")]
        [Describe("Argentine style Flank Steak with bleu cheese, garlic, and Worcestershire sauce")]
        FlankSteak,
        [Describe("Cedar Planked Salmon with Shiitake Mushroom Ragout")]
        Salmon,
    };

    // TODO: Friendly responses
    [Serializable]
    public class DinnerOption
    {
        [Prompt("What would {GuestName} like to eat? {||}")]
        public PlateOptions? PlateOption;

        public string GuestName;

        public static IForm<DinnerOption> BuildForm()
        {
            return new FormBuilder<DinnerOption>().Build();
        }
    }
}