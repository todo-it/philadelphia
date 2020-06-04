namespace Philadelphia.Web {
    /// <summary>
    /// assumes that form starts hidden
    /// </summary>
    public interface ITitleFormCanvasStrategy {
        string Title { set;  }
        void OnCanvasHiding();
        void OnCanvasShowing();
    }
}
