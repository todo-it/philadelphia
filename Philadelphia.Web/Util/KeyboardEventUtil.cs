using Bridge.Html5;

namespace Philadelphia.Web {
    public static class KeyboardEventUtil {
        public static bool IsNavigationKey(this KeyboardEvent self) {
            return 
                self.KeyCode == 16 || //shift
                self.KeyCode == 17 || //ctrl
                self.KeyCode == 18 || //alt
                self.KeyCode == 9  || //tab
                self.KeyCode == 38 || //up arrow
                self.KeyCode == 40 || //down arrow
                self.KeyCode == 37 || //left arrow
                self.KeyCode == 39 || //right arrow
                self.KeyCode == 36 || //home
                self.KeyCode == 35 || //end
                self.KeyCode == 33 || //page up
                self.KeyCode == 34;   //page down
        } 
    }
}
