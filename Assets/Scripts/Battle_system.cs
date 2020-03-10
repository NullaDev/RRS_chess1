using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Battle_system : MonoBehaviour
{
    //战斗系统，控制卡牌的加载和整个战斗流程
    private GameObject prfb;
    private GameObject transmission_ob;//transmission_object的实体
    private Data_transmission trans;//transmission_object实体的Data_transmission组件
    // private Data_transmission transmission;
    private GameObject temporary;//初始化时的临时变量
    private GameObject temp;//战斗时的临时变量
    private Card_controller cc;//临时变量
    private List<GameObject> player_queue = new List<GameObject>();//玩家队列
    private List<GameObject> enemy_queue = new List<GameObject>();//敌人队列
    private List<GameObject> offensive_queue;//攻方队列
    private List<GameObject> defensive_queue;//守方队列

    public Text round;//轮次转换的提示文本
    public Text end;//战斗结束的提示文本

    [SerializeField] private int adr = -1;//防守方下标，在一次攻击结束后会被重置为-1
    private bool pause = false;//暂停信号
    private int num_of_round;//轮次

    public int sb = 1;
    public int of_0_id = 7;
    public Battle_system()
    {
        
    }
    void Start()
    {
        Debug.Log("Debug.start");

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

        Interrupt(4, false);//开始之前暂停四秒，用于显示轮次信息和欣赏卡牌（x

        adr = -1;//重置防守方下标
    }
    public void Loading()
    {
        int i;
        prfb = (GameObject)Resources.Load("Card/Card_controller");//预设资源获取
        for (i=0;i<trans.p_amt;i++)//加载玩家方的卡牌实体并入队
        {
            temporary = (GameObject)Instantiate(prfb);//加载预设生成实体
            temporary.SetActive(true);
            temporary.GetComponent<Transform>().position = new Vector2(-17.8f + (i + 1) * 35.6f / (trans.p_amt + 1), -5f);//定位
            cc = temporary.GetComponent<Card_controller>();
            //加载乱七八糟的信息
            cc.Set_lct(temporary.GetComponent<Transform>().position);
            cc.Gs_type = 0;
            cc.Gs_id = trans.ci.card_information[trans.Get_card(i)].ID;
            cc.Gs_describe = trans.ci.card_information[trans.Get_card(i)].Describe;
            cc.Gs_level = trans.ci.card_information[trans.Get_card(i)].Level;
            cc.Gs_atk = trans.ci.card_information[trans.Get_card(i)].ATK;
            cc.Gs_hp = trans.ci.card_information[trans.Get_card(i)].HP;
            cc.Gs_order = i;
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
            cc.Gs_id = trans.ci.card_information[trans.Get_enemy(i)].ID;
            cc.Gs_describe = trans.ci.card_information[trans.Get_enemy(i)].Describe;
            cc.Gs_level = trans.ci.card_information[trans.Get_enemy(i)].Level;
            cc.Gs_atk = trans.ci.card_information[trans.Get_enemy(i)].ATK;
            cc.Gs_hp = trans.ci.card_information[trans.Get_enemy(i)].HP;
            cc.Gs_order = i;
            enemy_queue.Add(temporary);
        }

        //Locate(player_queue,-5f);
        //Locate(enemy_queue,5f);
    }

    // Update is called once per frame
    void Update()
    {
        //每帧都判定一次是否触发xxx事件，串行判定，每帧至多只能触发一次事件。比如：执行完卡牌销毁后就不能在这一帧重置队列，要等待暂停时间到再进行下一事件的判定
        if (pause)//暂停判定
        {
            return;
        }
        if(player_queue.Count != 0 && enemy_queue.Count != 0)
        {
            //若没有一方全部阵亡
            int unattacked = player_queue.Count + enemy_queue.Count;
            //判定是否所有卡牌都攻击过一遍，若是，结束本轮，重置所有卡牌状态，等待下一轮开始，若不是跳过
            for (int i = 0; i < player_queue.Count; i++)
            {
                if (player_queue[i].GetComponent<Card_controller>().Gs_attacked == true && player_queue[i].GetComponent<Card_controller>().Gs_back == true)
                {
                    unattacked--;
                }
            }
            for (int i = 0; i < enemy_queue.Count; i++)
            {
                if (enemy_queue[i].GetComponent<Card_controller>().Gs_attacked == true && enemy_queue[i].GetComponent<Card_controller>().Gs_back == true)
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
                num_of_round++;
                round.text = "Round" + num_of_round.ToString();
                round.gameObject.SetActive(true);
                Invoke("Close_round", 3);
                Interrupt(4, false);
                Debug.Log("The queue has been inited.");
            }
            else
            {
            }
            if (pause) return;//暂停判定

            if ((adr != -1) && (offensive_queue[0].GetComponent<Card_controller>().Gs_dead == true||defensive_queue[adr].GetComponent<Card_controller>().Gs_dead == true
                || (offensive_queue[0].GetComponent<Card_controller>().Gs_attacked == true && offensive_queue[0].GetComponent<Card_controller>().Gs_back == false)))
            {
                //若一次攻击已结束
                int a = 0;
                if (defensive_queue[adr].GetComponent<Card_controller>().Gs_dead == true)
                {
                    //若防守方死亡，销毁防守方，将其移除队列，重置防守方卡牌位置
                    temp = defensive_queue[adr];
                    a = temp.GetComponent<Card_controller>().Gs_order;
                    defensive_queue.RemoveAt(adr);
                    temp.GetComponent<Card_controller>().Gs_destroy = true;
                    Relocate(defensive_queue,a);
                    Debug.Log("Offenser changed because of defenser's death.");
                }

                if (offensive_queue[0].GetComponent<Card_controller>().Gs_dead == true)
                {
                    //若攻击方死亡，销毁攻击方，将其移除队列，重置攻击方卡牌位置
                    temp = offensive_queue[0];
                    a = temp.GetComponent<Card_controller>().Gs_order;
                    offensive_queue.RemoveAt(0);
                    temp.GetComponent<Card_controller>().Gs_destroy = true;
                    Relocate(offensive_queue,a);
                    Debug.Log("Offenser changed because of offenser's death.");
                }
                else if (offensive_queue[0].GetComponent<Card_controller>().Gs_attacked == true && offensive_queue[0].GetComponent<Card_controller>().Gs_back == false)
                {
                    //若无人死亡，攻击方返回其原本位置，将其放置在攻击队列队尾
                    temp = offensive_queue[0];
                    offensive_queue.RemoveAt(0);
                    offensive_queue.Add(temp);
                    Debug.Log("Offenser changed because the attack ended.");
                }

                adr = -1;//重置防守方下标
                //攻守方交换
                List<GameObject> temp_queue = offensive_queue;
                offensive_queue = defensive_queue;
                defensive_queue = temp_queue;
                Debug.Log("Offenser change preparing");
                //调试信息
                if (offensive_queue.Count!=0)
                {
                    Debug.Log("Offenser changing");
  
                      of_0_id = offensive_queue[0].GetComponent<Card_controller>().Gs_id;
                    Debug.Log("Offenser changed to" + offensive_queue[0].GetComponent<Card_controller>().Gs_id + ".");
                }   
                Interrupt(4, false);//暂停4秒，等待销毁或归位
            }
            else if (adr == -1)
            {
                //如果防守方下标已重置，代表将开始下一次攻击，随机选择防守卡牌
                System.Random rd = new System.Random();
                adr = (int)rd.Next(0, defensive_queue.Count);
                Debug.Log("offence_id = " + offensive_queue[0].GetComponent<Card_controller>().Gs_id + ",defense_id = " + defensive_queue[adr].GetComponent<Card_controller>().Gs_id);
                //启动碰撞体（不参与战斗的卡牌没有碰撞体）
                offensive_queue[0].GetComponent<BoxCollider2D>().enabled = true;
                defensive_queue[adr].GetComponent<BoxCollider2D>().enabled = true;
                //设置信号
                defensive_queue[adr].GetComponent<Card_controller>().Gs_defense = true;
                offensive_queue[0].GetComponent<Card_controller>().Gs_back = false;
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
                //暂停0.5秒，让卡牌飞一会
                Interrupt(0.5f, false);
                Debug.Log("Offenser " + offensive_queue[0].GetComponent<Card_controller>().Gs_id + "'s new enemy is" + adr);
            }
            if (pause)
            {
                Debug.Log("3333333333333333");
                return; 
            }
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
    private void Close_round()//关闭轮次信息
    {
        round.gameObject.SetActive(false);
    }
    private void Relocate(List<GameObject> l,int order)//重定位一方的所有卡牌，在有单位死亡时调用，参数order为死亡卡牌的编号
    {
        for (int i = 0; i < l.Count; i++)
        {
            if(l[i].GetComponent<Card_controller>().Gs_order> order)//重排攻击次序
            {
                l[i].GetComponent<Card_controller>().Gs_order -= 1;
            }
            //重新定位
            l[i].GetComponent<Transform>().position = new Vector2(-17.8f + (l[i].GetComponent<Card_controller>().Gs_order + 1) * 35.6f / (l.Count + 1), l[i].GetComponent<Transform>().position.y);
            l[i].GetComponent<Card_controller>().Set_lct(l[i].GetComponent<Transform>().position);
        }
    }
    private void Locate(List<GameObject> l,float f)//定位一方的所有卡牌，在初始化时调用
    {
        for (int i = 0; i < l.Count; i++)
        {
            l[i].GetComponent<Transform>().position = new Vector2(-17.8f + (i + 1) * 35.6f / (l.Count + 1), f);
            l[i].GetComponent<Card_controller>().Set_lct(l[i].GetComponent<Transform>().position);
        }
    }
    public void Interrupt(float time,bool forever)//中断模块，参数1为中断事件，参数2为是否永久中断
    {
        pause = true;//将暂停信号设置为1，update方法中运行到暂停判定处时就会直接返回
        if (!forever)//若不是永久中断，time秒后结束中断，恢复update的运行
        {
            Invoke("Interrupt_end", time);
        }
        /*for (int i = 0; i < offensive_queue.Count; i++)
        {

        }*/
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
}
