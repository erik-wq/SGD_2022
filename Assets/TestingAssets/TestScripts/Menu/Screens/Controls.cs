using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : BaseScreen
{
   public void Back()
   {
        Hide();
        GameData.menuManager.Show<Main>();
   }
}
