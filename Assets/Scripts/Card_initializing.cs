using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card_initializing 
{
    //卡牌库初始类
    public Card_initializing()
    {
        a = 6;
        for(int i = 0;i<amount;i++)
        {
            Card_type c = new Card_type();
            c.Setting(i,describe[i],level[i],ATK[i],HP[i]);
            card_information.Add(c);
            a = card_information[i].ATK;
        }
    }
    public readonly List<Card_type> card_information = new List<Card_type>();//卡牌信息库，加载卡牌实体时从这里获得卡牌信息，只读，不可更改
    public int amount = 5;
    //初始化信息数组，只读，不可更改
    private readonly string[] describe = new string[5] {"高町奈叶","菲特","维塔", "希格纳姆", "莎玛尔"};
    private readonly int[] level = new int[5]{3,3,2,2,2};
    private readonly int[] ATK = new int[5]{4,3,2,3,4};
    private readonly int[] HP = new int[5]{9,10,8,7,6};
    public int a =7;
    
    public List<Card_type> get_card_information()
    {
        return card_information;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
