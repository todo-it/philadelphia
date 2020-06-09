using System;

namespace Philadelphia.Web {
    public static class Magics {
        public const string ProgramaticCloseFormEventName = "closeform";
        
        public const double MinUserResizeColumnWidth = 7.0;
        public const double MaxUserResizeColumnWidth = 1400;
        public const float DefaultColumnWidthPx = 100;
        public const int DefaultDataGridHeightPx = 200;
        public const double DefaultDataGridRowHeight = 20;
        public const int DefaultDropDownVisibleItems = 6;

        public const int AutocompleteDefaultDelay = 150;
        public const int TooltipFadeInMs = 300;
        public const int TooltipFadeOutMs = 1000;
        public const int ValidationTriggerDelayMilisec = 150;
        public const int MenuBarBorder = 16;
        
        public const float ExtraWidthAddedToPrototypePx = 15;
        
        public const int KeyCodeBackspace = 8;
        public const int KeyCodeTab = 9;
        public const int KeyCodeShift = 16;
        
        public const int KeyCodeEscape = 27;
        public const int KeyCodeEnter = 13;
        public const int KeyCodeArrowLeft = 37;
        public const int KeyCodeArrowRight = 39;
        public const int KeyCodeArrowUp = 38;
        public const int KeyCodeArrowDown = 40;
        public const int KeyCodeZero = 48;
        public const int KeyCodeNine = 57;
        public const int KeyCodeNumpadZero = 96;
        public const int KeyCodeNumpadNine = 105;

        public const string PostReturningFileParameterName = "i";
        
        public const string AttrDataEnvironment = "data-environment";
        
        public const string AttrDataFormContainer = "data-form";
        public const string AttrDataFormId = "data-formId";
        public const string AttrDataFormIsPopup = "data-isPopup";
        public const string AttrDataFormIsShown = "data-shown";
        public const string AttrDataFormIsCloseable = "data-closeable";
        public const string AttrDataFormHeader = "data-formHeader";
        public const string AttrDataFormTitle = "data-formTitle";
        public const string AttrDataFormBody = "data-formBody";
        public const string AttrDataFormActions = "data-formActions";
        public const string AttrDataFormDefaultAction = "data-defaultAction";

        public const string AttrDataHandlesEnter = "data-handlesEnter";
        public const string AttrDataOptOutOfWholeTextSelectionOnFocus = "data-noSelectOnFocus";
        public const string AttrDataForId = "data-forId";
        public const string AttrDataMutable = "data-mutable";
        public const string AttrDataNonMutable = "data-nonMutable";
        public const string AttrDataEmptyCaption = "data-emptyCaption";
        public const string AttrDataDraggingFile = "data-dragging";
        
        //TODO refactor to avoid traversing (expensive operation)
        public const string AttrDataNotraverse = "data-notraverse";
        
        public const string AttrDataIsHorizontalPanel = "data-horizontalPanel";
        public const string AttrDataIsVerticalPanel = "data-verticalPanel";
        
        
        public const string AttrDataErrorsTooltip = "data-errors-tooltip";
        public const string AttrDataDisabledTooltip = "data-disabled-tooltip";
        public const string AttrDataMayBeTooltipContainer = "data-is-container";
        public const string AttrDataMouseEventRecipient = "data-mouseevent";
        public const string AttrDataResizeRecipient = "data-resizeevent";
        public const string AttrDataMenuItemId = "data-menuitemid";
        public const string AttrDataInnerHtml = "data-innerText";
        public const string AttrDataSortOrder = "data-sort";
        
        public const string AttrAlignToRight = "data-alignToRight";
        public const string AttrDataSelectionHandler = "data-selectHndlr";
        public const string AttrDataReadOnly = "data-readonly";
        public const string AttrDataIcon = "data-icon";
        public const string AttrDataStatus = "data-status";

        public const string CssClassOptions = "options";
        public const string CssClassGlass = "glass";
        public const string CssClassLightBox = "lightBox";
        public const string CssClassLightBoxLoading = "loading";
        public const string CssClassLightBoxLoaded = "loaded";
        public const string CssClassLightBoxThrobber = "throbber";
        public const string CssClassLightBoxClose = "close";
        public const string CssClassLightBoxOpenInNewTab = "openInNewTab";

        public const string CssClassGroupOrAggregate = "groupOrAggregate";
        public const string CssClassAggregateFunc = "aggregateFunc";
        public const string CssClassGroupingFunc = "groupFunc";
        public const string CssClassSettingsAction = "settingsAction";
        public const string CssClassScratched = "scratched";
        public const string CssClassLink = "link";
        public const string CssClassPreserveNewlines = "preserveNewlines";
        public const string CssClassFirstRowIsOdd = "firstRowIsOdd";
        public const string CssClassIsSelectionHandler = "selectionHndlr";
        public const string CssClassTooltip = "tooltip";
        public const string CssClassTooltipContainer = "tooltipContainer";
        public const string CssClassInactive = "inactive";
        public const string CssClassActive = "active";
        public const string CssClassDisabled = "disabled";
        public const string CssClassEnabled = "enabled";
        
        //TODO rework to leverage margin-left: auto where possible
        //HtmlTableBasedTableView is not possible at this moment as reload button is display-none-able in runtime and thus margin-left: auto doesn't work
        public const string CssClassFlexSpacer = "flexSpacer";
        public const string CssClassSplitter = "splitter";
        public const string CssClassSearchBox = "searchBox";
        public const string CssClassPressed = "pressed";
        public const string CssClassPopup = "popup";
        
        public const string CssClassExtraElement = "extraElement";
        public const string CssClassTabHandleContainer = "tabHandleContainer";
        public const string CssClassTabContentContainer = "tabContentContainer";
        public const string CssClassHideAction = "hideAction";
        public const string CssClassShowAction = "showAction";
        public const string CssClassPositionRelative = "positionRelative";
        public const string CssClassItems = "items";

        public const string CssClassIcon = "icon";
        
        public const string CssClassFontAwesomeBasedButton = "usesFontBtn";
        public const string CssClassFontAwesomeBasedButtonLabelLess = "usesFontBtnLabelLess";
        
        public const string CssClassFilterRemove = "filterRemove";
        public const string CssClassFilterable = "filterable";
        public const string CssClassFilterMainContainer = "filterMainContainer";
        public const string CssClassFilterActionContainer = "filterActionContainer";
        public const string CssClassGroupingRemove = "groupRemove";
        public const string CssClassAggregationRemove = "aggregationRemove";

        public const string CssClassFilter = "filter";
        public const string CssClassWithFilter = "withFilter";
        public const string CssClassWithGrouping = "withGrouping";
        public const string CssClassWithAggregation = "withAggregation";
        public const string CssClassFilterIndicator = "filterIndicator";
        public const string CssClassGroupIndicator = "groupIndicator";
        public const string CssClassAggregationIndicator = "aggregationIndicator";
        public const string CssClassIsValidating = "validating";
        public const string CssClassRowSelected = "selectedRow";
        public const string CssClassRowActivated = "activatedRow";
        public const string CssClassDatagridAction = "datagridAction";
        public const string CssClassAnchorWithFontIcon = "anchorWithFontIcon";
        public const string CssClassRunning = "running";
        public const string CssClassFailed = "failed";
        public const string CssClassIsEmpty = "isEmpty";
        public const string CssClassIsClearable = "isClearable";
        public const string CssClassClearContainer = "clearContainer";
        public const string CssClassTableLike = "tableLike";
        public const string CssClassNotRendered = "notRendered";
        public const string CssClassValidationState = "validationState";
        public const string CssClassResizeHandle = "resizeHandle";
        public const string CssClassIsResizing = "isResizing";
        public const string CssClassColumnLabel = "columnLabel";
        public const string CssClassHeaderClose = "headerClose";
        public const string CssClassHeaderTitle = "headerTitle";
        public const string CssClassUploadActions = "uploadActions";
        
        public const string CssClassCurrent = "current";
        public const string CssClassValidDay = "validDay";
        public const string CssClassInvalidDay = "invalidDay";
        public const string CssClassDaysOfMonth = "daysOfMonth";
        public const string CssClassFormerMonthDay = "formerMonthDay";
        public const string CssClassThisMonthDay = "thisMonthDay";
        public const string CssClassNextMonthDay = "nextMonthDay";
        public const string CssClassActiveFormerMonth = "activeFormerMonth";
        public const string CssClassActiveThisMonth = "activeThisMonth";
        public const string CssClassActiveNextMonth = "activeNextMonth";
        public const string CssClassToday = "today";

        public const string CssClassYearAndMonthChoice = "yearAndMonthChoice";
        public const string CssClassYearAndMonthName = "yearAndMonthName";
        public const string CssClassPopupContainer = "popupContainer";
        public const string CssClassPopups = "popups";
        public const string CssClassChoosen = "choosen";
        public const string CssClassInRange = "inRange";
        public const string CssClassSince = "since";
        public const string CssClassUntil = "until";
        public const string CssClassDateTimeFormatFixedText = "fixedText";
        public const string CssClassWrongDateComponent = "wrong";
        public const string CssClassUpArrow = "upArrow";
        public const string CssClassDownArrow = "downArrow";
        
        public const string CssClassInputContainer = "inputContainer";
        public const string CssClassClickToEditable = "clickToEditable";
        public const string CssClassEditing = "editing";
        public const string CssClassViewing = "viewing";
        public const string CssClassTextVersion = "textVersion";
        public const string CssClassValue = "value";
        public const string CssClassValueContainer = "valueContainer";
        public const string CssClassTabHandle = "tabHandle";
        public const string CssClassTabLabel = "tabLabel";
        public const string CssClassTabContent = "tabContent";
        public const string CssClassTdMayOverflow = "tdMayOverflow";
        
        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomePrint = FontAwesomeSolid.IconPrint;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeDownload = FontAwesomeSolid.IconDownload;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeUpload = FontAwesomeSolid.IconUpload;
    
        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeBackward = FontAwesomeSolid.IconBackward;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeExclamationTriangle = FontAwesomeSolid.IconExclamationTriangle;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeChevronLeft = FontAwesomeSolid.IconChevronLeft;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeChevronRight = FontAwesomeSolid.IconChevronRight;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeForward = FontAwesomeSolid.IconForward;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeCalendar = FontAwesomeRegular.IconCalendarAlt;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeFilter = FontAwesomeSolid.IconFilter;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomePaperPlaneO = FontAwesomeSolid.IconPaperPlane;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeSortOrderUnspecified = FontAwesomeSolid.IconSort;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeSortOrderAsc = FontAwesomeSolid.IconSortAlphaDown;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeSortOrderDesc = FontAwesomeSolid.IconSortAlphaDownAlt;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomePlusCircle = FontAwesomeSolid.IconPlusCircle;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeSearch = FontAwesomeSolid.IconSearch;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeClose = FontAwesomeSolid.IconTimes;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeArrowsAlt = FontAwesomeSolid.IconArrowsAlt;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeReloadData = FontAwesomeSolid.IconSync;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeGears = FontAwesomeSolid.IconCogs;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeBarChart = FontAwesomeSolid.IconChartBar;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeTable = FontAwesomeSolid.IconTable;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeWindowClose = FontAwesomeSolid.IconTimes;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeSignOut = FontAwesomeSolid.IconSignOutAlt;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeBars = FontAwesomeSolid.IconBars;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeTrash = FontAwesomeSolid.IconTrashAlt;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeExchange = FontAwesomeSolid.IconExchangeAlt;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeEyeSlash = "";

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeTimes = FontAwesomeSolid.IconTimes;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeListUl = FontAwesomeSolid.IconListUl;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeFileTextO = FontAwesomeSolid.IconFileAlt;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeSpinner = FontAwesomeSolid.IconSpinner;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeThumbsoUp = FontAwesomeRegular.IconThumbsUp;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeThumbsUp = FontAwesomeSolid.IconThumbsUp;

        [Obsolete("use FontAwesome* class directly")]
        public const string FontAwesomeFloppyO = FontAwesomeRegular.IconSave;


        public const string IconUrlFilterDelete = "FilterDelete.png";
        public const string IconUrlExit = "Exit.png";
        public const string IconUrlSpinnerBig = "spin84.gif";
        public const int IconUrlSpinnerWidth = 84;
        public const int IconUrlSpinnerHeight = 84;

        public const string IconUrlExportToXlsx = "Xlsx.png";
        public const double ScrollingPxToIgnore = 5;

        public const string PurposeMouseMove = "mousemove";
        public const int UploadViewDefaultActionsPx = 18;
        public const int UploadViewDefaultThumbWidth = 180;
        public const int UploadViewDefaultThumbHeight = 120;
        public const string UnicodeHorizontalEllipsis = "…";
        public const int DefaultLightBoxOuterMargin = 20;
    }
}
