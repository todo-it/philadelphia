using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class TwoVerticalPanelsWithResizer : TwoPanelsWithResizer {
        private TwoVerticalPanelsWithResizer(
                Hideability hideable, Tuple<int?,int?> fixedSize, SpacingPolicy? policy, 
                int minPanelSizePx = DefaultPanelSizePx) 
                    : base(hideable, minPanelSizePx, fixedSize, policy) {

            Container.SetAttribute(Magics.AttrDataIsVerticalPanel, "");
        }

        public TwoVerticalPanelsWithResizer(
            Hideability hideable, Tuple<int?,int?> fixedSize, int minPanelSizePx = DefaultPanelSizePx) 
                : this(hideable, fixedSize, null, minPanelSizePx) {}

        public TwoVerticalPanelsWithResizer(Hideability hideable, SpacingPolicy policy = SpacingPolicy.Proportional,
            int minPanelSizePx = DefaultPanelSizePx) : this(hideable, null, policy, minPanelSizePx) {}

        protected override void SetPanelsSize(Tuple<double,double> sizes) {
            FirstPanel.Style.FlexBasis = $"{sizes.Item1}px";
            FirstPanel.Style.Height = $"{sizes.Item1}px"; //needed for children using height 100%
            FirstPanel.Style.MaxHeight = $"{sizes.Item1}px"; //needed for FF

            SecondPanel.Style.FlexBasis = $"{sizes.Item2}px";
            SecondPanel.Style.Height = $"{sizes.Item2}px"; //needed for children using height 100%
            SecondPanel.Style.MaxHeight = $"{sizes.Item2}px"; //needed for FF
        }

        protected override Tuple<double,double> CalculateSizesOnAttachOrResize(VisibilityAction change, Tuple<double,double> lastSizeOrNull = null) {
           
            var theoretAvailSpace = Container.GetAvailableHeightForFormElement();
            var factAvailSpace = theoretAvailSpace - Splitter.GetBoundingClientRect().Height;

            var upperHght = FirstPanel.GetBoundingClientRect().Height;
            var lowerHght = SecondPanel.GetBoundingClientRect().Height;
            
            Logger.Debug(GetType(), "lastSize change={0} lastSize={1} avail={2}",
                change, lastSizeOrNull?.Item1 + lastSizeOrNull?.Item2, factAvailSpace);

            if (lastSizeOrNull != null && change == VisibilityAction.Showing && 
                    DoubleExtensions.AreApproximatellyTheSame(
                        lastSizeOrNull.Item1 + lastSizeOrNull.Item2, factAvailSpace, 1.1)) {

                Logger.Debug(GetType(), "reusing lastSize");

                upperHght = lastSizeOrNull.Item1;
                lowerHght = lastSizeOrNull.Item2;
            }  else if (change == VisibilityAction.Showing) {
                Logger.Debug(GetType(), "not reusing lastSize");
                var res = ComputeSpace(upperHght, lowerHght, factAvailSpace);
                
                upperHght = res.Item1;
                lowerHght = res.Item2;
            } else {
                if (Hideable == Hideability.First) {
                    upperHght = 0;
                    lowerHght = factAvailSpace;
                } else if (Hideable == Hideability.Second) {
                    upperHght = factAvailSpace;
                    lowerHght = 0;
                }
            }
            
            if (change == VisibilityAction.Showing) {
                upperHght = Math.Max(upperHght, MinPanelSizePx);
                lowerHght = Math.Max(lowerHght, MinPanelSizePx);
                
                //make sure that panels are visible on show
                var ratio = upperHght/(upperHght+lowerHght);
                upperHght = factAvailSpace * ratio;
                lowerHght = factAvailSpace - upperHght;
            }

            Logger.Debug(GetType(), "CalculateSizesInitial(id={0}) availSpace={1} splitterHeight={2} outcome=({3},{4})", 
                Container.Id, theoretAvailSpace, Splitter.GetBoundingClientRect().Height, upperHght, lowerHght);

            return Tuple.Create(upperHght, lowerHght);
        }

        protected override Tuple<double,double> CalculateSizesOnUserResize(int pageX, int pageY) {
            var availSpace = Container.GetAvailableHeightForFormElement();
            
            var basePosY = Container.OffsetTop;
            var handlePos = Splitter.GetBoundingClientRect();
                
            double upperHght = pageY - basePosY;
            var lowerHght = availSpace + basePosY - pageY - handlePos.Height;
                
            Logger.Debug(GetType(), "CalculateSizesOnEvent availSpace={0} splitterHeight={1} outcome=({2},{3})", 
                availSpace, handlePos.Height, upperHght, lowerHght);

            return Tuple.Create(upperHght, lowerHght);
        }
    }
}
