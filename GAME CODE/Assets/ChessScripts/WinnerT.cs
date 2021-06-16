using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WinnerT : BoardManager
{

    public TextMeshProUGUI Winner;
   
    public void Start()
    {

        if (win == 1)
        {

            Winner.text = "Black Wins!!";


        }
        else
        {
            Winner.text = "White Wins!!";
        }
    
    
    
    
    
    }

  
}
