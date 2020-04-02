using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card_initializing
{
    
    private const int battlecry = 0;
    //private const int battling = 1;//可合并为attacking和defensing
    private const int attacking = 2;
    private const int defensing = 3;
    private const int hurt = 4;//未完成
    private const int treated = 5;//未完成
    private const int start_of_turn = 6;
    private const int die = 7;

    //卡牌库初始类
    public Card_initializing()
    {
        a = 6;
        for(int i = 0;i<amount;i++)
        {
            Card_type c = new Card_type();
            c.Setting(i,describe[i],level[i],ATK[i],HP[i]);
            for (int j = 0; j < 8; j++)
            {
                c.skills[j] = new Skill();
            }
            card_information.Add(c);
            a = card_information[i].ATK;
        }
        //蒂安娜进攻时敌方随机打6
        card_information[0].skills[attacking].enable = true;
        card_information[0].skills[attacking].target[0] = 4;
        card_information[0].skills[attacking].type[0] = 2;
        card_information[0].skills[attacking].value[0] = 6;
        //菲特受伤时提升3攻击
        card_information[1].skills[hurt].enable = true;
        card_information[1].skills[hurt].target[0] = 1;
        card_information[1].skills[hurt].type[0] = 3;
        card_information[1].skills[hurt].value[0] = 3;
        //高町奈叶交战时全场打3
        card_information[2].skills[attacking].enable = true;
        card_information[2].skills[attacking].target[0] = 6;
        card_information[2].skills[attacking].type[0] = 2;
        card_information[2].skills[attacking].value[0] = 3;

        card_information[2].skills[defensing].enable = true;
        card_information[2].skills[defensing].target[0] = 6;
        card_information[2].skills[defensing].type[0] = 2;
        card_information[2].skills[defensing].value[0] = 3;
        //中岛昴回合结束时回3血并提升3攻击力
        card_information[3].skills[start_of_turn].enable = true;
        card_information[3].skills[start_of_turn].target[0] = 1;
        card_information[3].skills[start_of_turn].type[0] = 1;
        card_information[3].skills[start_of_turn].value[0] = 3;

        card_information[3].skills[start_of_turn].enable = true;
        card_information[3].skills[start_of_turn].target[1] = 1;
        card_information[3].skills[start_of_turn].type[1] = 3;
        card_information[3].skills[start_of_turn].value[1] = 3;
        //琳芙斯亡语敌方随机打10
        card_information[4].skills[die].enable = true;
        card_information[4].skills[die].target[0] = 4;
        card_information[4].skills[die].type[0] = 2;
        card_information[4].skills[die].value[0] = 10;
        //维塔攻击时打5
        card_information[5].skills[attacking].enable = true;
        card_information[5].skills[attacking].target[0] = 2;
        card_information[5].skills[attacking].type[0] = 2;
        card_information[5].skills[attacking].value[0] = 5;
        //八神疾风回合结束全场打4
        card_information[6].skills[start_of_turn].enable = true;
        card_information[6].skills[start_of_turn].target[0] = 6;
        card_information[6].skills[start_of_turn].type[0] = 2;
        card_information[6].skills[start_of_turn].value[0] = 4;
        //希格纳姆防守时随机提升己方攻击力3
        card_information[7].skills[start_of_turn].enable = true;
        card_information[7].skills[start_of_turn].target[0] = 3;
        card_information[7].skills[start_of_turn].type[0] = 3;
        card_information[7].skills[start_of_turn].value[0] = 3;
        //莎玛尔回合结束己方全体奶3
        card_information[8].skills[start_of_turn].enable = true;
        card_information[8].skills[start_of_turn].target[0] = 5;
        card_information[8].skills[start_of_turn].type[0] = 1;
        card_information[8].skills[start_of_turn].value[0] = 3;
    }
    /*
    public bool enable = false;//本卡牌是否拥有此阶段技能
    public int[] target = new int[3];//技能施放的目标，1为本身，2为攻击对象，3为己方随机，4为敌方随机，5为己方全体，6为敌方全体
    public int[] type = new int[3];//技能的效果类型，1为治疗，2为伤害，3为提升攻击力，4为降低攻击力，5为增加血量上限，6为减少血量上限，7为毁灭
    public int[] value = new int[3];//技能的数值
    */
    public readonly List<Card_type> card_information = new List<Card_type>();//卡牌信息库，加载卡牌实体时从这里获得卡牌信息，只读，不可更改
    public int amount = 9;
    //初始化信息数组，只读，不可更改
    private readonly string[] describe = new string[9] {"蒂安娜","菲特", "高町奈叶", "中岛昴","琳芙斯","维塔", "八神疾风", "希格纳姆", "莎玛尔"};
    private readonly int[] level = new int[9]{2,4,4,2,2,3,4,3,2};
    private readonly int[] ATK = new int[9]{4,7,7,4,2,7,8,6,2};
    private readonly int[] HP = new int[9]{15,30,25,20,25,25,20,30,15};
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
