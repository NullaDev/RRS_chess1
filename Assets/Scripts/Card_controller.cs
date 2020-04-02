using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card_controller : MonoBehaviour
{
    class Change
    {
        public int which;//0为ATK 1为HP 2为HP_limit
        public int trend;//0加1减
        public int vl;//值
    }
    private List<Change> changes = new List<Change>();
    private SpriteRenderer spr;

    private Card_type ctype;//卡牌数据载体
    private int type;//0为主站者所用卡牌，否则为敌人
    //[SerializeField] private int card_id;//是哪张牌，在场景切换时由主站者选择
    [SerializeField] private int order;//在队列里的攻击顺序
    [SerializeField] private int eternal_order;//队列初始化时在队列里的攻击顺序，不会随着队列成员的增加或减少而改变
    [SerializeField] private int HP_limit;//生命值上限
    [SerializeField] private Vector2 location;//本卡牌应在的位置，用于攻击后返回本位和位置重置

    [SerializeField] private bool[] signal = new bool[5];//信号组，用于描述本卡牌目前的状态和控制行动流程
    //信号0表示是否已攻击过，1代表在本轮已攻击过，0代表本轮还未攻击
    //信号1表示是否在进行防御，1代表被选中为攻击对象，进行防御，0代表碰撞已结束，结束本次防御
    //信号2表示是否已完成动作归位，1代表在本位，0代表不在本位，在攻击/返回动作中
    //信号3表示是否应死亡（应死亡不代表已销毁，要等待战斗系统发送销毁信号），1代表应死亡，0代表不应
    //信号4表示是否已接受战斗系统发回的销毁信号
    private const int attacked = 0;
    private const int defense = 1;
    private const int back = 2;
    private const int dead = 3;
    private const int destroy = 4;
    private bool pause = false;//暂停信号
    private bool pause1 = false;//暂停信号

    public int ATK_before;
    public int HP_before;
    public int HP_limit_before;

    //卡牌文本的底板
    private GameObject Describe_board;
    private GameObject Level_board;
    private GameObject ATK_board;
    private GameObject HP_board;
    private GameObject Change_board;

    //卡牌文本，canv是画布，Inform_text是文本组件的数组
    private GameObject canv;
    private Text[] Inform_text = new Text[5];

    //调试用临时变量
    public int sb;
    public string nimabi;
    public float cnm = 0.1f;
    public Vector2 size1;
    public Vector2 size2;

    //卡面相关图片资源的根目录
    private string picturepath = "Card_face/";
    private void Add_change(int wch,int num,int num_before)
    {
        if(num!=num_before)
        {
            Debug.Log("我是" + ctype.Describe + ",有数值变更" + wch + "的" + num_before + "to" + num);
            Change change = new Change();
            change.which = wch;
            if (num < num_before)
                change.trend = 0;
            else
                change.trend = 1;
            change.vl = num - num_before;
            changes.Add(change);
        }
    }
    private void Card_face_loading(GameObject g,string s)//加载卡面的方法，用于各个部分的图片加载
    {
        nimabi = s;
        Texture2D img = Resources.Load(picturepath + s) as Texture2D;//获取图片
        if (img != null)
        {
            Sprite sp = Sprite.Create(img, new Rect(0, 0, img.width, img.height), new Vector2(0.5f, 0.5f));//制作sprite,自适应图片宽高并居中
            g.GetComponent<SpriteRenderer>().sprite = sp;//设置卡面
        }
        else { /*异常*/}
    }
    public void Load()//加载卡牌信息
    {   
        //加载卡面
        spr = gameObject.GetComponent<SpriteRenderer>();
        Texture2D img = Resources.Load(picturepath + "background") as Texture2D;//获取图片
        if (img != null)
        {
            Sprite sp = Sprite.Create(img, new Rect(0, 0, img.width, img.height), new Vector2(0.5f, 0.5f));//制作sprite,自适应图片宽高并居中
            gameObject.GetComponent<SpriteRenderer>().sprite = sp;//设置卡面
            //设置碰撞体的大小和位置，使其自适应于卡面
            gameObject.GetComponent<BoxCollider2D>().transform.position = transform.position;
            gameObject.GetComponent<BoxCollider2D>().size = gameObject.GetComponent<SpriteRenderer>().bounds.size;
        }
        else { /*异常*/}
        
        //设置组件
        Level_board = gameObject.transform.GetChild(1).gameObject;
        Card_face_loading(Level_board, "level");//加载图片
        Level_board.transform.position = new Vector2(transform.position.x - (spr.bounds.size.x - Level_board.GetComponent<SpriteRenderer>().bounds.size.x) / 2,
            transform.position.y+(spr.bounds.size.y - Level_board.GetComponent<SpriteRenderer>().bounds.size.y) / 2);//调整位置

        ATK_board = gameObject.transform.GetChild(2).gameObject;
        Card_face_loading(ATK_board, "ATK");
        ATK_board.transform.position = new Vector2(transform.position.x - (spr.bounds.size.x - ATK_board.GetComponent<SpriteRenderer>().bounds.size.x) / 2,
            transform.position.y - (spr.bounds.size.y - ATK_board.GetComponent<SpriteRenderer>().bounds.size.y) / 2);

        HP_board = gameObject.transform.GetChild(3).gameObject;
        Card_face_loading(HP_board, "HP");
        HP_board.transform.position = new Vector2(transform.position.x + (spr.bounds.size.x - HP_board.GetComponent<SpriteRenderer>().bounds.size.x) / 2,
            transform.position.y - (spr.bounds.size.y - HP_board.GetComponent<SpriteRenderer>().bounds.size.y) / 2);

        Describe_board = gameObject.transform.GetChild(0).gameObject;
        Card_face_loading(Describe_board, "describe");
        Describe_board.transform.position = new Vector2(transform.position.x,
            transform.position.y - (spr.bounds.size.y - Describe_board.GetComponent<SpriteRenderer>().bounds.size.y - 2* ATK_board.GetComponent<SpriteRenderer>().bounds.size.y ) / 2);

        Change_board = gameObject.transform.GetChild(4).gameObject;
        Change_board.transform.position = gameObject.transform.position;

        //获取数值信息的gameobject
        canv = Instantiate(Resources.Load("Text/Canvas1") as GameObject);

        //获取text组件
        Inform_text = canv.GetComponentsInChildren<Text>();

        //设置文本信息
        Inform_text[0].text = ctype.Describe;
        Inform_text[1].text = ctype.Level.ToString();
        Inform_text[2].text = ctype.ATK.ToString();
        Inform_text[3].text = ctype.HP.ToString();
        Inform_text[4].text = null;

        //设置初始数据
        HP_limit = ctype.HP;
        ATK_before = ctype.ATK;
        HP_before = ctype.HP;
        HP_limit_before = HP_limit;
        State_init();
    }
    void Start()
    {
        Invoke("Load",0.5f);//延迟0.5秒加载，给战斗系统初始化其它组件流夏时间
        Interrupt(0,0.7f, false);//延迟0.7秒运行update，防止在加载成功之前运行出错  
    }

    // Update is called once per frame
    void Update()
    {
        if (pause)
        {
            //Debug.Log("11111111111111111");
            return;
        }

        if (signal[back])
            transform.position = location;

        //实时更新卡牌信息
        Inform_text[0].text = ctype.Describe;
        Inform_text[1].text = ctype.Level.ToString();
        Inform_text[2].text = ctype.ATK.ToString();
        Inform_text[3].text = ctype.HP.ToString();

        Add_change(0, ctype.ATK, ATK_before);
        Add_change(1, ctype.HP, HP_before);
        Add_change(2, HP_limit, HP_limit_before);
            
        ATK_before = ctype.ATK;
        HP_before = ctype.HP;
        HP_limit_before = HP_limit;

        //实时更新文本底板的位置，使其跟踪卡牌本身，并保持相对位置不变
        Level_board.transform.position = new Vector2(transform.position.x - (spr.bounds.size.x - Level_board.GetComponent<SpriteRenderer>().bounds.size.x) / 2,
            transform.position.y + (spr.bounds.size.y - Level_board.GetComponent<SpriteRenderer>().bounds.size.y) / 2);

        ATK_board.transform.position = new Vector2(transform.position.x - (spr.bounds.size.x - ATK_board.GetComponent<SpriteRenderer>().bounds.size.x) / 2,
            transform.position.y - (spr.bounds.size.y - ATK_board.GetComponent<SpriteRenderer>().bounds.size.y) / 2);

        HP_board.transform.position = new Vector2(transform.position.x + (spr.bounds.size.x - HP_board.GetComponent<SpriteRenderer>().bounds.size.x) / 2,
            transform.position.y - (spr.bounds.size.y - HP_board.GetComponent<SpriteRenderer>().bounds.size.y) / 2);

        Describe_board.transform.position = new Vector2(transform.position.x,
            transform.position.y - (spr.bounds.size.y - Describe_board.GetComponent<SpriteRenderer>().bounds.size.y - ATK_board.GetComponent<SpriteRenderer>().bounds.size.y * 2) / 2);

        Change_board.transform.position = gameObject.transform.position;

        //实时更新文本组件的位置，使其与底板位置保持一致
        Inform_text[0].transform.position =  Camera.main.WorldToScreenPoint(Describe_board.transform.position);
        Inform_text[1].transform.position = Camera.main.WorldToScreenPoint(Level_board.transform.position);
        Inform_text[2].transform.position = Camera.main.WorldToScreenPoint(ATK_board.transform.position);
        Inform_text[3].transform.position = Camera.main.WorldToScreenPoint(HP_board.transform.position);
        Inform_text[4].transform.position = Camera.main.WorldToScreenPoint(Change_board.transform.position);

        if (signal[destroy])
        {
            //如果接受到销毁信号，延迟0.1秒销毁本卡牌实体和文本实体
            //Invoke("Boom",0.1f);
            Boom();
        }

        if (signal[attacked] && !(System.Math.Abs(transform.position.y) < System.Math.Abs(location.y)))
        {   //如果已攻击过且已回到本位，设置速度为0，设置归位信号为1，设置渲染图层为普通卡牌（战斗中卡牌会在普通卡牌上方）
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
            spr.sortingLayerName = "card_face";
            for(int j=0;j<4;j++)
            {
                gameObject.transform.GetChild(j).GetComponent<SpriteRenderer>().sortingLayerName = "card_information";
            }
            signal[back] = true;
        }
        if (changes.Count != 0)
        {
            if (pause1)
                return;
            Card_face_loading(Change_board, "change" + changes[changes.Count - 1].which + changes[changes.Count - 1].trend);//加载图片
            //Debug.Log("我是" + ctype.Describe + ",请求加载图片" + "change" + changes[changes.Count - 1].which + changes[changes.Count - 1].trend);
            Change_board.SetActive(true);
            Debug.Log("我是" + ctype.Describe + ",数值发生改变，数值变化量为" + changes[changes.Count - 1].vl.ToString());
            Inform_text[4].text = changes[changes.Count - 1].vl.ToString();
            Interrupt(1,1, false);
        } 
        else
        {
            Inform_text[4].text = null;
        }
    }
    /*private void Change_end()
    {
        Change_board.SetActive(false);
    }*/
    private void Boom()
    {
        //销毁卡牌，文本底板和文本模块
        Debug.Log("我是" + ctype.Describe + " 我即将被销毁");
        Destroy(canv);
        Destroy(gameObject);
    }
    public void State_init()
    {
        //信号初始化
        for(int i = 0; i < 4;i++)
        {
            signal[i] = false;
        }
        signal[back] = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //碰撞时执行，结算伤害
        sb = 1000;
        Card_controller enemy = collision.gameObject.GetComponent<Card_controller>();//获取敌人
        ctype.HP = ctype.HP - enemy.Gs_ctype.ATK;//扣血
        Debug.Log("我是" + ctype.Describe + " 我因碰撞而受到"+ enemy.Gs_ctype.ATK+"点伤害");
        if (!(ctype.HP > 0))
        {
            //如果血扣光，停止运动，设置死亡信号为1，等待销毁。
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            signal[dead] = true;
            Debug.Log("我是" + ctype.Describe+ " 我即将阵亡");
        }
        else
        {
            //如果没死
            if (!signal[defense] && !signal[back])
            {
                //如果本次战斗中本张卡牌为攻击方
                signal[attacked] = true;//标记为已攻击
                gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2((location.x - transform.position.x) / 2, (location.y - transform.position.y) / 2);//归位移动
                Debug.Log("我是" + ctype.Describe + " 本次攻击结束，我即将归位");
            }
            else
            {
                //如果本次战斗中本张卡牌为防御方
                dedefense();//结束防御
            } 
        }
        gameObject.GetComponent<BoxCollider2D>().enabled = false;

    }
    private void dedefense()
    {
        signal[defense] = false;
        Debug.Log("我是" + ctype.Describe + "本次防御结束");
    }
    public void Interrupt(int p, float time, bool forever)//中断，详见战斗系统类
    {
        if (p == 0)
            pause = true;
        else if (p == 1)
            pause1 = true;
        if (!forever)
            Invoke("Interrupt_end", time);
    }
    private void Interrupt_end()
    {
        if(changes.Count!=0)
            changes.RemoveAt(changes.Count - 1);
        Change_board.SetActive(false);
        pause = false;
        pause1 = false;
    }
    //private成员的get,set方法
    public void Setsignal(int which,bool b)
    {
        if (which > -1 && which < 5)
            signal[which] = b;
        else
        {
            //异常
        }
    }
    public bool Getsignal(int which)
    {
        if (which > -1 && which < 5)
            return signal[which];
        else
        {
            //异常
            return false;
        }
    }
    public int Gs_type
    {
        get { return type; }
        set { type = value; }
    }
    public int Gs_order
    {
        get { return order; }
        set { order = value; }
    }
    public int Gs_eternal_order
    {
        get { return eternal_order; }
        set { eternal_order = value; }
    }
    public Card_type Gs_ctype
    {
        get { return ctype; }
        set { ctype = value; }
    }
    public int Gs_hp_limit
    {
        get { return HP_limit; }
        set { HP_limit = value; }
    }
    public void Set_lct(Vector2 l)
    { this.location = new Vector2(l.x, l.y); }
    public Vector2 Get_lct()
    { return this.location; }
}

