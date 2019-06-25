using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DateTimeDemoFormView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions => ActionsBuilder.For();        
        private readonly HorizontalTabbedView _tabbedView;
        public DateTimeRangeView RangeY {get; } = new DateTimeRangeView("Y range", "From", "To", 
            DateTimeFormat.Y, 
            I18n.GetDateTimeFormatForPrecision(DateTimeFormat.Y),
            new Tuple<DateTime?,DateTime?>(DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1)),
            Tuple.Create<DateTime?,DateTime?>(null, DateTimeExtensions.BuildSoonestMidnight()),
            PopupLocation.Right,
            LocalDateTimeBuilder.InstanceBeginOfDay,
            LocalDateTimeBuilder.InstanceEndOfDay);

        public DateTimeRangeView RangeYM {get; } = new DateTimeRangeView("YM range", "From", "To", 
            DateTimeFormat.YM, 
            I18n.GetDateTimeFormatForPrecision(DateTimeFormat.YM),
            new Tuple<DateTime?,DateTime?>(DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1)),
            Tuple.Create<DateTime?,DateTime?>(null, DateTimeExtensions.BuildSoonestMidnight()),
            PopupLocation.Right,
            LocalDateTimeBuilder.InstanceBeginOfDay,
            LocalDateTimeBuilder.InstanceEndOfDay);

        public DateTimeRangeView RangeYMD {get; } = new DateTimeRangeView("YMD range", "From", "To", 
            DateTimeFormat.DateOnly, 
            I18n.GetDateTimeFormatForPrecision(DateTimeFormat.DateOnly),
            new Tuple<DateTime?,DateTime?>(DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1)),
            Tuple.Create<DateTime?,DateTime?>(null, DateTimeExtensions.BuildSoonestMidnight()),
            PopupLocation.Right,
            LocalDateTimeBuilder.InstanceBeginOfDay,
            LocalDateTimeBuilder.InstanceEndOfDay );

        public DateTimeRangeView RangeYMDhm {get; } = new DateTimeRangeView("YMDhm range", "From", "To", 
            DateTimeFormat.YMDhm, 
            I18n.GetDateTimeFormatForPrecision(DateTimeFormat.YMDhm),
            new Tuple<DateTime?,DateTime?>(DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1)),
            Tuple.Create<DateTime?,DateTime?>(null, DateTimeExtensions.BuildSoonestMidnight()),
            PopupLocation.Right,
            LocalDateTimeBuilder.InstanceBeginOfDay,
            LocalDateTimeBuilder.InstanceEndOfDay);

        public DateTimePickerView SingleYEntry { get; } = DateTimePickerView.CreateSoleEntry(
            "Y single", 
            DateTimeFormat.Y, 
            I18n.GetDateTimeFormatForPrecision(DateTimeFormat.Y),
            DateTime.Now, 
            Tuple.Create<DateTime?,DateTime?>(null,DateTimeExtensions.BuildSoonestMidnight()),
            LocalDateTimeBuilder.InstanceBeginOfDay);

        public DateTimePickerView SingleYMEntry { get; } = DateTimePickerView.CreateSoleEntry(
            "YM single", 
            DateTimeFormat.YM, 
            I18n.GetDateTimeFormatForPrecision(DateTimeFormat.YM),
            DateTime.Now, 
            Tuple.Create<DateTime?,DateTime?>(null, DateTimeExtensions.BuildSoonestMidnight()),
            LocalDateTimeBuilder.InstanceBeginOfDay);

        public DateTimePickerView SingleYMDEntry { get; } = DateTimePickerView.CreateSoleEntry(
            "YMD single", 
            DateTimeFormat.DateOnly, 
            I18n.GetDateTimeFormatForPrecision(DateTimeFormat.DateOnly),
            DateTime.Now, 
            Tuple.Create<DateTime?,DateTime?>(null, DateTimeExtensions.BuildSoonestMidnight()),
            LocalDateTimeBuilder.Custom(
                y => DateTimeExtensions.IsToday(y) 
                ? y.DateOnly().WithTime(23, 59, 59) 
                : y.DateOnly().WithTime(12, 0, 0) ) );

        public DateTimePickerView SingleYMDhmEntry { get; } = DateTimePickerView.CreateSoleEntry(
            "YMDhm single", 
            DateTimeFormat.YMDhm, 
            I18n.GetDateTimeFormatForPrecision(DateTimeFormat.YMDhm),
            DateTime.Now, 
            Tuple.Create<DateTime?,DateTime?>(null, DateTimeExtensions.BuildSoonestMidnight()),
            LocalDateTimeBuilder.Custom(
                y => DateTimeExtensions.IsToday(y) 
                    ? y.DateOnly().WithTime(23, 59, 59) 
                    : y.DateOnly().WithTime(12, 0, 0) ));

        public DateTimePickerView SingleValidation { get; } = DateTimePickerView.CreateSoleEntry(
                "Datetime with even day&hour", 
                DateTimeFormat.YMDhm, 
                I18n.GetDateTimeFormatForPrecision(DateTimeFormat.YMDhm),
                null,
                Tuple.Create<DateTime?,DateTime?>(null, null),
                LocalDateTimeBuilder.InstanceBeginOfDay)
            .With(x => x.Widget.Style.Display = Display.Flex);

        public DateTimeRangeView RangeValidation { get; } = 
            new DateTimeRangeView("Three weekdays", "From", "To", 
                DateTimeFormat.DateOnly, 
                I18n.GetDateTimeFormatForPrecision(DateTimeFormat.DateOnly),
                new Tuple<DateTime?,DateTime?>(null, null),
                Tuple.Create<DateTime?,DateTime?>(null, null),
                PopupLocation.Right,
                LocalDateTimeBuilder.InstanceBeginOfDay,
                LocalDateTimeBuilder.InstanceBeginOfDay);
        
        private const string HelpHtml = @"Dates after tomorrow are not available in order to demonstrate such feature in calendar<br>
Tab and shift-tab is supported with popups auto hiding&showing on focus change<br> 
Date time formats are fully customizable(think: builder.Add(datetimepart field) and builder.Add(""fixed text"")<br><br>";

        public DateTimeDemoFormView() {
            _tabbedView = HorizontalTabbedView
                .CreateGeneric(
                    Tuple.Create<string,Action<HTMLElement>>("Single", cntnr => cntnr.AppendAllChildren(
                        new HTMLSpanElement {InnerHTML = HelpHtml}.With(x => x.ClassList.Add("grayedOut")),
                        new HTMLDivElement().With(x => {
                            x.ClassList.Add(Magics.CssClassTableLike);
                            x.AppendAllChildren(
                                SingleYEntry.Widget, new HTMLBRElement(), SingleYMEntry.Widget, new HTMLBRElement(),
                                SingleYMDEntry.Widget, new HTMLBRElement(), SingleYMDhmEntry.Widget);
                        })
                    )),
                    Tuple.Create<string,Action<HTMLElement>>("Range", cntnr => cntnr.AppendAllChildren(
                        new HTMLSpanElement { InnerHTML = HelpHtml }.With(x => x.ClassList.Add("grayedOut")),
                        new HTMLDivElement().With(x => {
                            x.ClassList.Add(Magics.CssClassTableLike);
                            x.AppendAllChildren(
                                RangeY.Widget, new HTMLBRElement(), RangeYM.Widget, new HTMLBRElement(),
                                RangeYMD.Widget, new HTMLBRElement(), RangeYMDhm.Widget );}))),
                    Tuple.Create<string,Action<HTMLElement>>("Validation", cntnr => cntnr.AppendAllChildren(
                        new HTMLSpanElement {InnerHTML = "Custom input validation in action" }.With(x => x.ClassList.Add("grayedOut")),
                        SingleValidation.Widget.With(x => {
                            x.Style.Display = Display.Flex;
                            x.Style.AlignItems = AlignItems.Center; } ), 
                        new HTMLBRElement(),
                        RangeValidation.Widget.With(x => {
                            x.Style.Display = Display.Flex;
                            x.Style.AlignItems = AlignItems.Center; } ))) )
                .With(x => x.TabContentContainer.Style.MinWidth = "315px");
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {_tabbedView};
        }
    }
}
