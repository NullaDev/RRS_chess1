using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card_type
{
    //卡牌基类
    public int ID;//ID，是辨识卡牌的唯一变量
    public string Describe;//卡牌描述
    public int Level;//等级
    public int ATK;//攻击力
    public int HP;//生命值
    // Start is called before the first frame update
    public void Setting(int id,string describe,int level,int atk,int hp)//初始化方法
    {
        ID = id;
        Describe = describe;
        Level = level; ;
        ATK = atk;
        HP = hp;
    }
    public Card_type Clone()//克隆一个卡牌基类
    {
        return this.MemberwiseClone() as Card_type;
    }

}
