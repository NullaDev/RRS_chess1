
public class Skill
{
    public bool enable = false;//本卡牌是否拥有此阶段技能
    public int[] target = new int[3];//技能施放的目标，1为本身，2为攻击对象，3为己方随机，4为敌方随机，5为己方全体，6为敌方全体
    public int[] type = new int[3];//技能的效果类型，1为治疗，2为伤害，3为提升攻击力，4为降低攻击力，5为增加血量上限，6为减少血量上限，7为毁灭
    public int[] value = new int[3];//技能的数值
}
