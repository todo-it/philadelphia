using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class TwoHorizontalPanelsWithResizer : TwoPanelsWithResizer {
        private TwoHorizontalPanelsWithResizer(
                Hideability hideable, Tuple<int?,int?> fixedSize, SpacingPolicy? policy, 
                int minPanelSizePx = DefaultPanelSizePx) 
                    : base(hideable, minPanelSizePx, fixedSize, policy) {

            Container.SetAttribute(Magics.AttrDataIsHorizontalPanel, "");
        }
        
        public TwoHorizontalPanelsWithResizer(
                Hideability hideable, Tuple<int?,int?> fixedSize, int minPanelSizePx = DefaultPanelSizePx) 
                    : this(hideable, fixedSize, null, minPanelSizePx) {}

        public TwoHorizontalPanelsWithResizer(
            Hideability hideable, SpacingPolicy policy = SpacingPolicy.Proportional,
                int minPanelSizePx = DefaultPanelSizePx) : this(hideable, null, policy, minPanelSizePx) {}

        protected override void SetPanelsSize(Tuple<double,double> sizes) {
            FirstPanel.Style.FlexBasis = $"{sizes.Item1}px";
            FirstPanel.Style.Width = $"{sizes.Item1}px"; //needed for children using height 100%
            FirstPanel.Style.MaxWidth = $"{sizes.Item1}px"; //needed for FF

            SecondPanel.Style.FlexBasis = $"{sizes.Item2}px";
            SecondPanel.Style.Width = $"{sizes.Item2}px"; //needed for children using height 100%
            SecondPanel.Style.MaxWidth = $"{sizes.Item2}px"; //needed for FF
        }
        
        protected override Tuple<double,double> CalculateSizesOnAttachOrResize(VisibilityAction change, Tuple<double,double> lastSizeOrNull = null) {
            
            var theoretAvailSpace = Window.InnerWidth; //TODO change to equivalent of Container.GetAvailableHeightForFormElement();
            var factAvailSpace = theoretAvailSpace - Splitter.GetBoundingClientRect().Width;
            
            var leftWdth = FirstPanel.GetBoundingClientRect().Width;
            var rightWdth = SecondPanel.GetBoundingClientRect().Width;
            
            Logger.Debug(GetType(), "lastSize change={0} lastSize={1} avail={2}",
                change, lastSizeOrNull?.Item1 + lastSizeOrNull?.Item2, factAvailSpace);

            if (lastSizeOrNull != null && change == VisibilityAction.Showing && 
                DoubleExtensions.AreApproximatellyTheSame(
                    lastSizeOrNull.Item1 + lastSizeOrNull.Item2, factAvailSpace, 1.1)) {

                Logger.Debug(GetType(), "reusing lastSize");

                leftWdth = lastSizeOrNull.Item1;
                rightWdth = lastSizeOrNull.Item2;
            } else if (change == VisibilityAction.Showing) {
                Logger.Debug(GetType(), "not reusing lastSize");
                var res = ComputeSpace(leftWdth, rightWdth, factAvailSpace);
                
                leftWdth = res.Item1;
                rightWdth = res.Item2;
            } else {
                if (Hideable == Hideability.First) {
                    leftWdth = 0;
                    rightWdth = factAvailSpace;
                } else if (Hideable == Hideability.Second) {
                    leftWdth = factAvailSpace;
                    rightWdth = 0;
                }
            }

            if (change == VisibilityAction.Showing) {
                leftWdth = Math.Max(leftWdth, MinPanelSizePx);
                rightWdth = Math.Max(rightWdth, MinPanelSizePx);
                
                //make sure that panels are visible on show
                var ratio = leftWdth/(leftWdth+rightWdth);
                leftWdth = factAvailSpace * ratio;
                rightWdth = factAvailSpace - leftWdth;
            }

            Logger.Debug(GetType(), "CalculateSizesInitial(id={0})  availSpace={1} splitterHeight={2} outcome=({3},{4})", 
                Container.Id, theoretAvailSpace, Splitter.GetBoundingClientRect().Width, leftWdth, rightWdth);

            return Tuple.Create(leftWdth, rightWdth);
        }

        protected override Tuple<double,double> CalculateSizesOnUserResize(int pageX, int pageY) {
            var basePosX = Container.OffsetLeft;
            var availSpace = Container.GetBoundingClientRect();
            
            var handlePos = Splitter.GetBoundingClientRect();
                
            double leftWdth = pageX - basePosX;
            var rightWdth = availSpace.Width + basePosX - pageX - handlePos.Width;
                
            Logger.Debug(GetType(), "CalculateSizesOnEvent availSpace={0} splitterWidth={1} outcome=({2},{3})", 
                availSpace, handlePos.Height, leftWdth, rightWdth);

            return Tuple.Create(leftWdth, rightWdth);
        }
    }
}
