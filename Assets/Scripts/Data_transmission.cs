using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Data_transmission : MonoBehaviour
{
    //信息传递装置，在场景切换时不会消失
    private int[] card_queue = new int[7];//己方出战卡牌队列
    private int[] enemy_queue = new int[7];//敌方出战卡牌队列
    public Card_initializing ci;
    public int p_amt;//己方出战卡牌数目
    public int e_amt;//敌方出战卡牌数目
    public bool of_or_def;//先手权，为true时我方先手，为false时我方后手
    //唯一的卡牌信息获取途径
    //从地下城转到战斗的流程：遭遇敌人（meeting）→进入阵容选择界面（choosing）→战斗（battling）
    //                
    //进入商店后的流程：
    //进入商店（shoping）→←进入卡牌购买界面（buying）
    //         ↓↑
    //进入卡牌升级界面（choosing）
    public void Card_choosing(int[] card_num)//根据阵容选择阶段传递的信息设置己方卡牌的ID，after choosing
    {
        int i;
        for (i = 0;i<p_amt;i++)
        {
            card_queue[i] = card_num[i];
        }
        for (; i < 7; i++)
        {
            card_queue[i] = 0;
        }

    }
    public void Enemy_setting(int[] enemy_num)//根据地图传递的信息设置敌方卡牌的ID,after meeting
    {
        int i = 0;
        for (i = 0; i < e_amt; i++)
        {
            enemy_queue[i] = enemy_num[i];
        }
        for (; i < 7; i++)
        {
            enemy_queue[i] = 0;
        }
    }
    public int Get_card(int a)//读得所有所选的卡牌的ID，before battling
    {
        if(a < 7 && a > -1)
            return card_queue[a];
        else
        {
            //异常
            return 3;
        }
    }
    public int Get_enemy(int a)//读得所有敌人的ID，before battling
    {
        if (a < 7 && a > -1)
            return enemy_queue[a];
        else
        {
            //异常
            return 3;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);//此对象在场景切换时不会消失
        ci = new Card_initializing();
        //以下是用于测试的信息，后期会由数据库和阵容选择界面代替
        p_amt = 4;
        e_amt = 5;
        card_queue[0] = 0;
        card_queue[1] = 1;
        card_queue[2] = 2;
        card_queue[3] = 3;
        enemy_queue[0] = 4;
        enemy_queue[1] = 5;
        enemy_queue[2] = 6;
        enemy_queue[3] = 7;
        enemy_queue[4] = 8;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))//按E进入下一场景
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }//按E切换场景
    }
}
