using UnityEngine;
public class Card_type
{
    //卡牌基类
    public int ID;//ID，是辨识卡牌的唯一变量
    public string Describe;//卡牌描述
    public int Level;//等级
    public int ATK;//攻击力
    public int HP;//生命值
    public Skill[] skills = new Skill[8];//技能组的七个成员分别代表七个阶段
    //0为战吼，1为交战时，2为攻击时，3为被攻击时，4为受伤时，5为受到治疗时，6为每回合开始，7为亡语
    // Start is called before the first frame update
    public void Setting(int id,string describe,int level,int atk,int hp)//初始化方法
    {
        ID = id;
        Describe = describe;
        Level = level; 
        ATK = atk;
        HP = hp;
    }
    public Card_type Clone()//克隆一个卡牌基类
    {
        return this.MemberwiseClone() as Card_type;
    }
}
