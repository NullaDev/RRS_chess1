using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class Battle_system : MonoBehaviour
{
    //战斗系统，控制卡牌的加载和整个战斗流程
    private GameObject prfb;
    private GameObject transmission_ob;//transmission_object的实体
    private Data_transmission trans;//transmission_object实体的Data_transmission组件
    private GameObject temporary;//初始化时的临时变量
    //private GameObject temp;//战斗时的临时变量
    private Card_controller cc;//临时变量
    private List<GameObject> player_queue = new List<GameObject>();//玩家队列
    private List<GameObject> enemy_queue = new List<GameObject>();//敌人队列
    private List<GameObject> offensive_queue;//攻方队列
    private List<GameObject> defensive_queue;//守方队列

    [SerializeField] private int stage = 0;
    private const int battlecry = 0;
    //private const int battling = 1;//可合并为attacking和defensing
    private const int attacking = 2;
    private const int defensing = 3;
    private const int hurt = 4;//未完成
    private const int treated = 5;//未完成
    private const int start_of_turn = 6;
    private const int die = 7;

    private const int attacked = 0;
    private const int defense = 1;
    private const int back = 2;
    private const int dead = 3;
    private const int destroy = 4;

    public Text round;//轮次转换的提示文本
    public Text end;//战斗结束的提示文本

    [SerializeField] private int adr = -1;//防守方下标，在一次攻击结束后会被重置为-1
    private bool pause = false;//暂停信号
    private int num_of_round;//轮次
    private bool end_of_turn = false;
    private bool offenser_changing = false;
    [SerializeField] private bool offenser_dead = false;//在某时刻攻击者是否已死亡
    [SerializeField] private bool defenser_dead = false;//在某时刻被攻击者是否已死亡

    public int sb = 1;
    public Battle_system()
    {

    }
    private void Turn_start_skill()
    {
        for (int i = 0; i < offensive_queue.Count; i++)
        {
            Using_skill(start_of_turn, true, offensive_queue[i], 0);
        }
        for (int i = 0; i < defensive_queue.Count; i++)
        {
            Using_skill(start_of_turn, false, defensive_queue[i], 0);
        }
    }
    void Start()
    {
        Debug.Log("Debug开始");

        //显示round1
        num_of_round = 1;
        round.text = "Round" + num_of_round.ToString();
        round.gameObject.SetActive(true);
        Invoke("Close_round", 3);//轮次信息显示持续三秒钟

        transmission_ob = GameObject.Find("Data_transmission");//找到从地下城界面和阵容选择界面传输来的信息载体
        trans = transmission_ob.GetComponent<Data_transmission>();//获取信息载体的代码组件——Data_transmission类并生产对象

        Loading();//加载所有参与战斗的卡牌

        offensive_queue = trans.of_or_def ? player_queue : enemy_queue;//设置攻方队列
        defensive_queue = trans.of_or_def ? enemy_queue : player_queue;//设置防守方队列

        stage = battlecry;
        Battlecry();
        Invoke("Turn_start_skill", 2);

        Interrupt(4, false);//开始之前暂停四秒，用于显示轮次信息和欣赏卡牌（x

        adr = -1;//重置防守方下标
    }
    public void Loading()
    {
        int i;
        prfb = (GameObject)Resources.Load("Card/Card_controller");//预设资源获取
        for (i = 0; i < trans.p_amt; i++)//加载玩家方的卡牌实体并入队
        {
            temporary = (GameObject)Instantiate(prfb);//加载预设生成实体
            temporary.SetActive(true);
            temporary.GetComponent<Transform>().position = new Vector2(-17.8f + (i + 1) * 35.6f / (trans.p_amt + 1), -5f);//定位
            cc = temporary.GetComponent<Card_controller>();
            //加载乱七八糟的信息
            cc.Set_lct(temporary.GetComponent<Transform>().position);
            cc.Gs_type = 0;
            cc.Gs_order = i;
            cc.Gs_eternal_order = i;
            cc.Gs_ctype = Deep_copy(trans.ci.card_information[trans.Get_card(i)]);
            player_queue.Add(temporary);//加入玩家队列
        }
        for (i = 0; i < trans.e_amt; i++)//加载敌人的卡牌实体并入队
        {
            temporary = (GameObject)Instantiate(prfb);
            temporary.SetActive(true);
            temporary.GetComponent<Transform>().position = new Vector2(-17.8f + (i + 1) * 35.6f / (trans.e_amt + 1), 5f);
            cc = temporary.GetComponent<Card_controller>();
            cc.Set_lct(temporary.GetComponent<Transform>().position);
            cc.Gs_type = 1;
            cc.Gs_order = i;
            cc.Gs_eternal_order = i;
            cc.Gs_ctype = Deep_copy(trans.ci.card_information[trans.Get_enemy(i)]);
            enemy_queue.Add(temporary);
        }
    }
    // Update is called once per frame
    void Update()
    {
        //每帧都判定一次是否触发xxx事件，串行判定，每帧至多只能触发一次事件。比如：执行完卡牌销毁后就不能在这一帧重置队列，要等待暂停时间到再进行下一事件的判定
        if (pause)//暂停判定
        {
            return;
        }
        if (player_queue.Count != 0 && enemy_queue.Count != 0)
        {
            //若没有一方全部阵亡
            int unattacked = player_queue.Count + enemy_queue.Count;
            //判定是否所有卡牌都攻击过一遍，若是，结束本轮，重置所有卡牌状态，等待下一轮开始，若不是跳过
            for (int i = 0; i < player_queue.Count; i++)
            {
                if (player_queue[i].GetComponent<Card_controller>().Getsignal(attacked) && player_queue[i].GetComponent<Card_controller>().Getsignal(back))
                {
                    unattacked--;
                }
            }
            for (int i = 0; i < enemy_queue.Count; i++)
            {
                if (enemy_queue[i].GetComponent<Card_controller>().Getsignal(attacked) && enemy_queue[i].GetComponent<Card_controller>().Getsignal(back))
                {
                    unattacked--;
                }
            }
            sb = unattacked;
            if (unattacked == 0)//若本轮已结束
            {
                //重置队列，轮次+1
                for (int i = 0; i < player_queue.Count; i++)
                {
                    player_queue[i].GetComponent<Card_controller>().State_init();
                }
                for (int i = 0; i < enemy_queue.Count; i++)
                {
                    enemy_queue[i].GetComponent<Card_controller>().State_init();
                }
                for (int i = 0; i < offensive_queue.Count; i++)
                {
                    Using_skill(start_of_turn, true, offensive_queue[i], 0);
                }
                for (int i = 0; i < defensive_queue.Count; i++)
                {
                    Using_skill(start_of_turn, false, defensive_queue[i], 0);
                }
                num_of_round++;
                round.text = "Round" + num_of_round.ToString();
                round.gameObject.SetActive(true);

                Invoke("Close_round", 3);
                Interrupt(4, false);
                end_of_turn = true;
                Debug.Log("队列初始化完毕");
            }
            else
            {
            }
            if (pause) return;//暂停判定

            //if ((adr != -1) && (offensive_queue[0].GetComponent<Card_controller>().Getsignal(dead) || defensive_queue[adr].GetComponent<Card_controller>().Getsignal(dead)
            //    || (offensive_queue[0].GetComponent<Card_controller>().Getsignal(attacked) && !offensive_queue[0].GetComponent<Card_controller>().Getsignal(back))))
            //{
            if (offenser_dead || defenser_dead
                || (offensive_queue[0].GetComponent<Card_controller>().Getsignal(attacked) && !offensive_queue[0].GetComponent<Card_controller>().Getsignal(back)))
            {
                if(offensive_queue[0].GetComponent<Card_controller>().Getsignal(attacked) && !offensive_queue[0].GetComponent<Card_controller>().Getsignal(back))
                {
                    Debug.Log("碰撞之前无人阵亡，碰撞成功触发");
                }
                //若一次攻击已结束
                /*int a = 0;
                if (defensive_queue[adr].GetComponent<Card_controller>().Getsignal(dead))
                {
                    //若防守方死亡，销毁防守方，将其移除队列，重置防守方卡牌位置
                    temp = defensive_queue[adr];
                    a = temp.GetComponent<Card_controller>().Gs_order;
                    defensive_queue.RemoveAt(adr);
                    temp.GetComponent<Card_controller>().Setsignal(destroy, true);
                    Relocate(defensive_queue, a);
                    Debug.Log("Offenser changed because of defenser's death.");
                }

                if (offensive_queue[0].GetComponent<Card_controller>().Getsignal(dead))
                {
                    //若攻击方死亡，销毁攻击方，将其移除队列，重置攻击方卡牌位置
                    temp = offensive_queue[0];
                    a = temp.GetComponent<Card_controller>().Gs_order;
                    offensive_queue.RemoveAt(0);
                    temp.GetComponent<Card_controller>().Setsignal(destroy, true);
                    Relocate(offensive_queue, a);
                    Debug.Log("Offenser changed because of offenser's death.");
                }*/
                //int[] death_result = new int[3];
                Death_decision();
                if (!offenser_dead && defensive_queue.Count != 0)
                {
                    if (!defenser_dead)
                        Using_skill(hurt, true, offensive_queue[0], defensive_queue[adr].GetComponent<Card_controller>().Gs_eternal_order);
                    else
                    { Using_skill(hurt, true, offensive_queue[0], defensive_queue[0].GetComponent<Card_controller>().Gs_eternal_order); }
                }
                if (!defenser_dead && offensive_queue.Count != 0)
                {
                    if (!offenser_dead)
                        Using_skill(hurt, true, defensive_queue[adr], offensive_queue[0].GetComponent<Card_controller>().Gs_eternal_order);
                    else
                    { Using_skill(hurt, true, defensive_queue[adr], offensive_queue[0].GetComponent<Card_controller>().Gs_eternal_order); }
                }
                //if (!offenser_dead && !defenser_dead && offensive_queue[0].GetComponent<Card_controller>().Getsignal(attacked) && !offensive_queue[0].GetComponent<Card_controller>().Getsignal(back))
                
                if(defenser_dead && !offenser_dead)
                {
                    offensive_queue[0].GetComponent<Card_controller>().Setsignal(attacked,true);//标记为已攻击
                    offensive_queue[0].GetComponent<Rigidbody2D>().velocity = offensive_queue[0].GetComponent<Card_controller>().Get_lct();
                    Debug.Log("我是" + offensive_queue[0].GetComponent<Card_controller>().Gs_ctype.Describe + " 本次攻击结束，我即将归位");
                }

                if (!offenser_dead && offensive_queue[0].GetComponent<Card_controller>().Getsignal(attacked) && !offensive_queue[0].GetComponent<Card_controller>().Getsignal(back))
                {
                    GameObject temp;
                    //若无人死亡，攻击方返回其原本位置，将其放置在攻击队列队尾
                    temp = offensive_queue[0];
                    offensive_queue.RemoveAt(0);
                    offensive_queue.Add(temp);
                    Debug.Log("攻击结束，攻击单位返回");
                }

                adr = -1;//重置防守方下标
                //攻守方交换
                if(!defensive_queue[0].GetComponent<Card_controller>().Getsignal(attacked))
                {
                    List<GameObject> temp_queue = offensive_queue;
                    offensive_queue = defensive_queue;
                    defensive_queue = temp_queue;
                    Debug.Log("攻守交换");
                }
                else
                    Debug.Log("防守方所有单位已全部完成本轮攻击，不进行攻守交换");
                offenser_dead = false;
                defenser_dead = false;
                Interrupt(4, false);//暂停4秒，等待销毁或归位
            }

            if (pause)
            {
                return;
            }

            if (adr == -1)
            {
                //调试信息
                if (offensive_queue.Count != 0)
                {
                    Debug.Log("切换攻击单位");
                    Debug.Log("攻击单位切换为" + offensive_queue[0].GetComponent<Card_controller>().Gs_ctype.Describe + ".");
                }
                //如果防守方下标已重置，代表将开始下一次攻击，随机选择防守卡牌
                System.Random rd = new System.Random();
                adr = (int)rd.Next(0, defensive_queue.Count);
                Debug.Log("攻击单位为" + offensive_queue[0].GetComponent<Card_controller>().Gs_ctype.Describe + ",被攻击单位为" + defensive_queue[adr].GetComponent<Card_controller>().Gs_ctype.Describe);
                //启动碰撞体（不参与战斗的卡牌没有碰撞体）
                offensive_queue[0].GetComponent<BoxCollider2D>().enabled = true;
                defensive_queue[adr].GetComponent<BoxCollider2D>().enabled = true;
                //设置信号
                defensive_queue[adr].GetComponent<Card_controller>().Setsignal(defense, true);
                offensive_queue[0].GetComponent<Card_controller>().Setsignal(back, false);
                //设置渲染图层为战斗卡牌（战斗中卡牌会在普通卡牌上方）
                offensive_queue[0].GetComponent<SpriteRenderer>().sortingLayerName = "battling_card_face";
                for (int j = 0; j < 4; j++)
                {
                    offensive_queue[0].transform.GetChild(j).GetComponent<SpriteRenderer>().sortingLayerName = "battling_card_information";
                }
                //设置速度，使其向防守方方向移动
                Vector2 vct = new Vector2((defensive_queue[adr].GetComponent<Transform>().position.x - offensive_queue[0].GetComponent<Transform>().position.x) / 2,
                    (defensive_queue[adr].GetComponent<Transform>().position.y - offensive_queue[0].GetComponent<Transform>().position.y) / 2);
                offensive_queue[0].GetComponent<Rigidbody2D>().velocity = vct;
                Using_skill(attacking, true, offensive_queue[0], defensive_queue[adr].GetComponent<Card_controller>().Gs_eternal_order);
                Using_skill(defensing, false, defensive_queue[adr], offensive_queue[0].GetComponent<Card_controller>().Gs_eternal_order);
                //暂停0.5秒，让卡牌飞一会
                Interrupt(0.5f, false);
                //Debug.Log("攻击单位" + offensive_queue[0].GetComponent<Card_controller>().Gs_ctype.Describe + "的攻击目标设置为" + defensive_queue[adr].GetComponent<Card_controller>().Gs_ctype.Describe);
            }
            if (pause)
            {
                Debug.Log("防守单位选择完成，下一次攻击即将开始");
                return;
            }
        }
        else if (enemy_queue.Count == 0 && player_queue.Count == 0)
        {
            //如果敌方全部阵亡，显示胜利文本
            Draw();
        }
        else if (enemy_queue.Count == 0)
        {
            //如果敌方全部阵亡，显示胜利文本
            Victory();
        }
        else if (player_queue.Count == 0)
        {
            //如果我方全部阵亡，显示失败文本
            Defeat();
        }
    }
    private void Relocate(List<GameObject> l, int order)//重定位一方的所有卡牌，在有单位死亡时调用，参数order为死亡卡牌的编号
    {
        Vector2 v;
        for (int i = 0; i < l.Count; i++)
        {
            if (l[i].GetComponent<Card_controller>().Gs_order > order)//重排攻击次序
            {
                l[i].GetComponent<Card_controller>().Gs_order -= 1;
            }
            //重新定位
            v = new Vector2(-17.8f + (l[i].GetComponent<Card_controller>().Gs_order + 1) * 35.6f / (l.Count + 1), l[i].GetComponent<Transform>().position.y);
            l[i].GetComponent<Card_controller>().Set_lct(v);
        }
    }
    private void Locate(List<GameObject> l, float f)//定位一方的所有卡牌，在初始化时调用
    {
        for (int i = 0; i < l.Count; i++)
        {
            l[i].GetComponent<Transform>().position = new Vector2(-17.8f + (i + 1) * 35.6f / (l.Count + 1), f);
            l[i].GetComponent<Card_controller>().Set_lct(l[i].GetComponent<Transform>().position);
        }
    }
    private void Battlecry()
    {
        Debug.Log("战吼发动中");
        int i;
        for (i = 0; i < offensive_queue.Count; i++)
        {
            Using_skill(battlecry, true, offensive_queue[i], 0);
        }
        for (i = 0; i < defensive_queue.Count; i++)
        {
            Using_skill(battlecry, false, defensive_queue[i], 0);
        }
        Debug.Log("战吼发动结束");
    }

    private int Death_decision()
    {
        Debug.Log("目前阶段为"+stage+"死亡判定开始");
        //bool offenser_dead = false;//在某时刻攻击者是否已死亡
        //bool defenser_dead = false;//在某时刻被攻击者是否已死亡
        bool others_dead = false;
        bool somebody_dead = false;
        int i = 0;
        for (i = 0; i < offensive_queue.Count; i++)//若攻击方有其它单位死亡，销毁攻，将其移除队列，重置攻击方卡牌位置
        {
            if(!(offensive_queue[i].GetComponent<Card_controller>().Gs_ctype.HP>0))
            {
                offensive_queue[i].GetComponent<Card_controller>().Setsignal(dead,true);
            }
            if (offensive_queue[i].GetComponent<Card_controller>().Getsignal(dead))
            {
                Debug.Log(offensive_queue[i].GetComponent<Card_controller>().Gs_ctype.Describe + "阵亡");
                somebody_dead = true;
            }
        }
        for (i = 0; i < defensive_queue.Count; i++)//若攻击方有其它单位死亡，销毁攻，将其移除队列，重置攻击方卡牌位置
        {
            if (!(defensive_queue[i].GetComponent<Card_controller>().Gs_ctype.HP>0))
            {
                defensive_queue[i].GetComponent<Card_controller>().Setsignal(dead, true);
            }
            if (defensive_queue[i].GetComponent<Card_controller>().Getsignal(dead))
            {
                Debug.Log(defensive_queue[i].GetComponent<Card_controller>().Gs_ctype.Describe + "阵亡");
                somebody_dead = true;
            }
        }
        if (!somebody_dead)
        {
            Debug.Log("无人阵亡，死亡判定结束");
            return 0;
        }

        GameObject temp;
        int a = 0;
        if (offensive_queue[0].GetComponent<Card_controller>().Getsignal(dead))
        {
            //若攻击单位死亡，销毁攻击单位，将其移除队列，重置攻击方卡牌位置
            temp = offensive_queue[0];
            a = temp.GetComponent<Card_controller>().Gs_order;
            offensive_queue.RemoveAt(0);
            if (adr != -1)
                Using_skill(die, true, temp, defensive_queue[adr].GetComponent<Card_controller>().Gs_eternal_order);
            else
                Using_skill(die, true, temp, -1);
            temp.GetComponent<Card_controller>().Setsignal(destroy, true);
            Relocate(offensive_queue, a);
            offenser_dead = true;
            Debug.Log("攻击单位阵亡，需要切换攻击单位");
        }

        if (adr != -1)
        {
            if (defensive_queue[adr].GetComponent<Card_controller>().Getsignal(dead))
            {
                //若防守单位死亡，销毁防守单位，将其移除队列，重置防守方卡牌位置
                temp = defensive_queue[adr];
                a = temp.GetComponent<Card_controller>().Gs_order;
                defensive_queue.RemoveAt(adr);
                adr = -1;
                Using_skill(die, false, temp, offensive_queue[0].GetComponent<Card_controller>().Gs_eternal_order);
                temp.GetComponent<Card_controller>().Setsignal(destroy, true);
                Relocate(defensive_queue, a);
                defenser_dead = true;
                Debug.Log("被攻击单位阵亡，需要切换攻击单位");
            }
        }

        for (i = 0; i < offensive_queue.Count; i++)//若攻击方有其它单位死亡，销毁攻，将其移除队列，重置攻击方卡牌位置
        {
            if (offensive_queue[i].GetComponent<Card_controller>().Getsignal(dead))
            {
                //Debug.Log("其它卡牌阵亡");
                temp = offensive_queue[i];
                a = temp.GetComponent<Card_controller>().Gs_order;
                offensive_queue.RemoveAt(i);
                Using_skill(die, true, temp, -1);
                temp.GetComponent<Card_controller>().Setsignal(destroy, true);
                Relocate(offensive_queue, a);
                Debug.Log(temp.GetComponent<Card_controller>().Gs_ctype.Describe + "阵亡");
                others_dead = true;
            }
        }
        for (i = 0; i < defensive_queue.Count; i++)//若防守方有其它单位死亡，销毁攻，将其移除队列，重置攻击方卡牌位置
        {
            if (defensive_queue[i].GetComponent<Card_controller>().Getsignal(dead))
            {
                //Debug.Log("another card of defensive camp has died.");
                temp = defensive_queue[i];
                
                a = temp.GetComponent<Card_controller>().Gs_order;
                defensive_queue.RemoveAt(i);
                Using_skill(die, false, temp, -1);
                temp.GetComponent<Card_controller>().Setsignal(destroy, true);
                Relocate(defensive_queue, a);
                Debug.Log(temp.GetComponent<Card_controller>().Gs_ctype.Describe + "阵亡");
                others_dead = true;
            }
        }
        if (offenser_dead && defenser_dead) return 1;
        else if (offenser_dead) return 2;
        else if (defenser_dead) return 3;
        else if (others_dead) return 4;
        else
        {
            Debug.Log("谁也没死");
            return 0;
        }

    }
    private int Using_skill(int s, bool camp, GameObject me, int objorder)
    {//camp表示现在正在施法的单位属于哪个阵营，true为进攻方，false为防守方，me为施法单位，enemy为施法单位此时正在对战的单位
        int stage_before = stage;
        stage = s;
        Debug.Log("我是" + me.GetComponent<Card_controller>().Gs_ctype.Describe + "目前阶段为" + stage + "开始发动技能");
        Card_controller mycontroller = me.GetComponent<Card_controller>();
        //Card_controller enemycontroller = enemy.GetComponent<Card_controller>();
        Skill sk = trans.ci.card_information[mycontroller.Gs_ctype.ID].skills[stage];//获取施法单位的技能
        //Debug.Log("I'm using skill,my description is" + trans.ci.card_information[mycontroller.Gs_ctype.ID].Describe);

        if (!sk.enable)
        {
            Debug.Log("没有可以发动的技能");
            return 0;
        }
        else
        {
            Debug.Log(mycontroller.Gs_ctype.Describe + "发动技能中");
        }


        bool target_flag = true;//技能对象的类型，true为单体，false为队列
        Card_controller skill_target = new Card_controller();//技能的单体对象
        List<GameObject> skill_target_queue = new List<GameObject>();//技能的队列对象
        List<GameObject> temp_queue;
        System.Random rdm = new System.Random();//随机数变量
        int temp;
        Card_controller temp_cd;
        for (int i = 0; i < sk.type.GetUpperBound(0); i++)
        {
            target_flag = true;
            /*switch (sk.target[i])
            {
                case 0:
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    target_flag = true;
                    break;
                case 5:
                case 6:
                    target_flag = false;
                    break;
                default:
                    //异常
                    break;
            }*/

            switch (sk.target[i])
            {

                case 0:
                    break;
                case 1:
                    skill_target = me.GetComponent<Card_controller>();
                    Debug.Log("我是" + me.GetComponent<Card_controller>().Gs_ctype.Describe + ",技能目标为自己");
                    break;
                case 2:
                    int k = 0;
                    if (camp)
                    {
                        for (k = 0; k < defensive_queue.Count; k++)
                        {
                            if (defensive_queue[k].GetComponent<Card_controller>().Gs_eternal_order == objorder && !defensive_queue[k].GetComponent<Card_controller>().Getsignal(dead))
                            {
                                skill_target = defensive_queue[k].GetComponent<Card_controller>();
                                Debug.Log("我是" + me.GetComponent<Card_controller>().Gs_ctype.Describe + ",技能目标为"+ skill_target.GetComponent<Card_controller>().Gs_ctype.Describe);
                                break;
                            }
                        }
                        if (k == defensive_queue.Count || skill_target.Getsignal(dead))
                            return 0;
                    }
                    else
                    {
                        for (k = 0; k < offensive_queue.Count; k++)
                        {
                            if (offensive_queue[k].GetComponent<Card_controller>().Gs_eternal_order == objorder&& !offensive_queue[k].GetComponent<Card_controller>().Getsignal(dead))
                            {
                                skill_target = offensive_queue[k].GetComponent<Card_controller>();
                                Debug.Log("我是" + me.GetComponent<Card_controller>().Gs_ctype.Describe + ",技能目标为" + skill_target.GetComponent<Card_controller>().Gs_ctype.Describe);
                                break;
                            }
                        }
                        if (k == offensive_queue.Count || skill_target.Getsignal(dead))
                            return 0;
                    }
                    break;
                case 3:
                    temp_queue = camp ? offensive_queue : defensive_queue;//此时施法单位在攻击方还是防守方？
                    temp = (int)rdm.Next(0, temp_queue.Count);
                    skill_target = temp_queue[temp].GetComponent<Card_controller>();
                    Debug.Log("我是" + me.GetComponent<Card_controller>().Gs_ctype.Describe + ",技能目标为" + skill_target.GetComponent<Card_controller>().Gs_ctype.Describe);
                    if (skill_target.Getsignal(dead))
                    { return 0; }
                    break;
                case 4:
                    temp_queue = camp ? defensive_queue : offensive_queue;//此时施法单位的对手在防守方还是攻击方？
                    temp = (int)rdm.Next(0, temp_queue.Count);
                    skill_target = temp_queue[temp].GetComponent<Card_controller>();
                    Debug.Log("我是" + me.GetComponent<Card_controller>().Gs_ctype.Describe + ",技能目标为" + skill_target.GetComponent<Card_controller>().Gs_ctype.Describe);
                    if (skill_target.Getsignal(dead))
                    { return 0; }
                    break;
                case 5:
                    skill_target_queue = camp ? offensive_queue : defensive_queue;//此时施法单位在攻击方还是防守方？
                    Debug.Log("我是" + me.GetComponent<Card_controller>().Gs_ctype.Describe + ",技能目标为" + skill_target_queue[0].GetComponent<Card_controller>().Gs_ctype.Describe+"所在阵营");
                    target_flag = false;
                    break;
                case 6:
                    skill_target_queue = camp ? defensive_queue : offensive_queue;//此时施法单位的对手在攻击方还是防守方？
                    Debug.Log("我是" + me.GetComponent<Card_controller>().Gs_ctype.Describe + ",技能目标为" + skill_target_queue[0].GetComponent<Card_controller>().Gs_ctype.Describe + "所在阵营");
                    target_flag = false;
                    break;
                default:
                    //异常
                    break;
            }
            Debug.Log("技能对象为" + sk.target[i]);
            Debug.Log("技能类型为" + sk.type[i]);
            Debug.Log("技能数值为" + sk.value[i]);
            int j = 0;
            switch (sk.type[i])
            {
                case 1:
                    if (target_flag)
                        skill_target.Gs_ctype.HP = skill_target.Gs_ctype.HP + sk.value[j];
                    else
                    {
                        for (j = 0; j < skill_target_queue.Count; j++)
                        {
                            temp_cd = skill_target_queue[j].GetComponent<Card_controller>();
                            temp_cd.Gs_ctype.HP = temp_cd.Gs_ctype.HP + sk.value[i];
                        }
                    }
                    break;
                case 2:
                    if (target_flag)
                    {
                        skill_target.Gs_ctype.HP = skill_target.Gs_ctype.HP - sk.value[i];
                    }
                    else
                    {
                        //Debug.Log("施法对象队列为" + skill_target_queue[0].GetComponent<Card_controller>().Gs_ctype.Describe+"所在队列");
                        for (j = 0; j < skill_target_queue.Count; j++)
                        {
                            temp_cd = skill_target_queue[j].GetComponent<Card_controller>();
                            temp_cd.Gs_ctype.HP = temp_cd.Gs_ctype.HP - sk.value[i];
                        }
                    }
                    break;
                case 3:
                    if (target_flag)
                        skill_target.Gs_ctype.ATK = skill_target.Gs_ctype.ATK + sk.value[i];
                    else
                    {
                        for (j = 0; j < skill_target_queue.Count; j++)
                        {
                            temp_cd = skill_target_queue[j].GetComponent<Card_controller>();
                            temp_cd.Gs_ctype.ATK = temp_cd.Gs_ctype.ATK + sk.value[i];
                        }
                    }
                    break;
                case 4:
                    if (target_flag)
                        skill_target.Gs_ctype.ATK = skill_target.Gs_ctype.ATK - sk.value[i];
                    else
                    {
                        for (j = 0; j < skill_target_queue.Count; j++)
                        {
                            temp_cd = skill_target_queue[j].GetComponent<Card_controller>();
                            temp_cd.Gs_ctype.ATK = temp_cd.Gs_ctype.ATK - sk.value[i];
                        }
                    }
                    break;
                case 5:
                    if (target_flag)
                        skill_target.Gs_hp_limit = skill_target.Gs_hp_limit + sk.value[i];
                    else
                    {
                        for (j = 0; j < skill_target_queue.Count; j++)
                        {
                            temp_cd = skill_target_queue[j].GetComponent<Card_controller>();
                            temp_cd.Gs_hp_limit = temp_cd.Gs_hp_limit + sk.value[i];
                        }
                    }
                    break;
                case 6:
                    if (target_flag)
                        skill_target.Gs_hp_limit = skill_target.Gs_hp_limit - sk.value[i];
                    else
                    {
                        for (j = 0; j < skill_target_queue.Count; j++)
                        {
                            temp_cd = skill_target_queue[j].GetComponent<Card_controller>();
                            temp_cd.Gs_hp_limit = temp_cd.Gs_hp_limit - sk.value[i];
                        }
                    }
                    break;
                case 7:
                    //毁灭
                    break;
                default:
                    //异常
                    break;
            }

        }
        stage = stage_before;
        Debug.Log("技能发动完毕");
        return Death_decision();
    }
    private void Close_round()//关闭轮次信息
    {
        round.gameObject.SetActive(false);
    }
    public void Interrupt(float time, bool forever)//中断模块，参数1为中断事件，参数2为是否永久中断
    {
        pause = true;//将暂停信号设置为1，update方法中运行到暂停判定处时就会直接返回
        if (!forever)//若不是永久中断，time秒后结束中断，恢复update的运行
        {
            Invoke("Interrupt_end", time);
        }
    }
    private void Interrupt_end()//中断结束模块
    {
        pause = false;
    }
    private void Victory()//胜利文本显示
    {
        Interrupt(0.1f, true);//永久中断update，战斗系统失效
        end.text = "恭喜你获得了对战胜利\n按B键返回地下城";
        end.gameObject.SetActive(true);
    }
    private void Defeat()//失败文本显示
    {
        Interrupt(0.1f, true);//永久中断update，战斗系统失效
        end.text = "很遗憾，对战失败了\n按B键返回地下城";
        end.gameObject.SetActive(true);
    }
    private void Draw()//平局文本显示
    {
        Interrupt(0.1f, true);//永久中断update，战斗系统失效
        end.text = "平局！\n按B键返回地下城";
        end.gameObject.SetActive(true);
    }
    public static T Deep_copy<T>(T obj)
    {
        if (obj is string || obj.GetType().IsValueType)
            return obj;

        object retval = Activator.CreateInstance(obj.GetType());
        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        for (int i = 0; i < fields.GetUpperBound(0); i++)
        {
            try
            {
                fields[i].SetValue(retval, Deep_copy(fields[i].GetValue(obj)));
            }
            catch { }
        }
        return (T)retval;
    }
}
