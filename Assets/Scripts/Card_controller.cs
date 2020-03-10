using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Card_controller : MonoBehaviour
{
    private SpriteRenderer spr;

    private int type;//0为主站者所用卡牌，否则为敌人
    //private int num;//编号,即图中的位置
    [SerializeField] private int card_id;//是哪张牌，在场景切换时由主站者选择
    [SerializeField] private int order;//在队列里的攻击顺序

    [SerializeField] private bool defense = false;//是否在进行防御，1代表被选中为攻击对象，进行防御，0代表碰撞已结束，结束本次防御
    [SerializeField] private bool attacked = false;//是否已攻击过，1代表在本轮已攻击过，0代表本轮还未攻击
    [SerializeField] private bool back = true;//是否已完成动作归位，1代表在本位，0代表不在本位，在攻击/返回动作中
    [SerializeField] private bool dead = false;//是否应死亡（应死亡不代表已销毁，要等待战斗系统发送销毁信号），1代表应死亡，0代表不应
    [SerializeField] private bool destroy = false;//是否已接受战斗系统发回的销毁信号
    [SerializeField] private bool pause = false;//暂停信号

    [SerializeField] private string Describe;//描述
    [SerializeField] private int Level;//等级
    [SerializeField] private int ATK;//攻击力
    [SerializeField] private int HP;//生命值

    private int Level_limit;//等级上限
    private int ATK_limit;//攻击力上限
    private int HP_limit;//生命值上限

    [SerializeField] private Vector2 location;//本卡牌应在的位置，用于攻击后返回本位和位置重置

    //卡牌文本的底板
    private GameObject Describe_board;
    private GameObject Level_board;
    private GameObject ATK_board;
    private GameObject HP_board;

    //卡牌文本，canv是画布，obj是Gameobject类实体，text是对应的文本组件
    private GameObject canv;
    private GameObject Describe_obj;
    private GameObject Level_obj;
    private GameObject ATK_obj;
    private GameObject HP_obj;

    private Text Describe_text;
    private Text Level_text;
    private Text ATK_text;
    private Text HP_text;

    //调试用临时变量
    public int sb;
    public string nimabi;
    public float cnm = 0.1f;
    public Vector2 size1;
    public Vector2 size2;

    //卡面相关图片资源的根目录
    private string picturepath = "Card_face/";
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
       
        //获取数值信息的gameobject
        canv = Instantiate(Resources.Load("Text/Canvas1") as GameObject);
        Describe_obj = canv.transform.GetChild(0).gameObject;
        Level_obj = canv.transform.GetChild(1).gameObject;
        ATK_obj = canv.transform.GetChild(2).gameObject;
        HP_obj = canv.transform.GetChild(3).gameObject;
        
        //获取text组件
        Describe_text = Describe_obj.GetComponent<Text>();
        Level_text = Level_obj.GetComponent<Text>();
        ATK_text = ATK_obj.GetComponent<Text>();
        HP_text = HP_obj.GetComponent<Text>();

        //设置文本信息
        Describe_text.text = Describe;
        Level_text.text = Level.ToString();
        ATK_text.text = ATK.ToString();
        HP_text.text = HP.ToString();

        //设置初始数据
        Level_limit = Level;
        ATK_limit = ATK;
        HP_limit = HP;
        State_init();
    }
    void Start()
    {
        Invoke("Load",0.5f);//延迟0.5秒加载，给战斗系统初始化其它组件流夏时间
        Interrupt(0.7f, false);//延迟0.7秒运行update，防止在加载成功之前运行出错
        
    }

    // Update is called once per frame
    void Update()
    {
        //实时更新卡牌信息
        Describe_text.text = Describe;
        Level_text.text = Level.ToString();
        ATK_text.text = ATK.ToString();
        HP_text.text = HP.ToString();

        //实时更新文本组件的位置，使其与底板位置保持一致
        Describe_text.transform.position =  Camera.main.WorldToScreenPoint(Describe_board.transform.position);
        Level_text.transform.position = Camera.main.WorldToScreenPoint(Level_board.transform.position);
        ATK_text.transform.position = Camera.main.WorldToScreenPoint(ATK_board.transform.position);
        HP_text.transform.position = Camera.main.WorldToScreenPoint(HP_board.transform.position);

        if (pause) return;

        if (destroy)
        {
            //如果接受到销毁信号，延迟0.1秒销毁本卡牌实体和文本实体
            Invoke("Boom",0.1f);
        }

        if (attacked == true && !(System.Math.Abs(transform.position.y) < System.Math.Abs(location.y)))
        {   //如果已攻击过且已回到本位，设置速度为0，设置归为信号为1，设置渲染图层为普通卡牌（战斗中卡牌会在普通卡牌上方）
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
            spr.sortingLayerName = "card_face";
            for(int j=0;j<4;j++)
            {
                gameObject.transform.GetChild(j).GetComponent<SpriteRenderer>().sortingLayerName = "card_information";
            }
            back = true;
        }
        
        //实时更新文本底板的位置，使其跟踪卡牌本身，并保持相对位置不变
        Level_board.transform.position = new Vector2(transform.position.x - (spr.bounds.size.x - Level_board.GetComponent<SpriteRenderer>().bounds.size.x) / 2,
            transform.position.y + (spr.bounds.size.y - Level_board.GetComponent<SpriteRenderer>().bounds.size.y) / 2);
        
        ATK_board.transform.position = new Vector2(transform.position.x - (spr.bounds.size.x - ATK_board.GetComponent<SpriteRenderer>().bounds.size.x) / 2,
            transform.position.y - (spr.bounds.size.y - ATK_board.GetComponent<SpriteRenderer>().bounds.size.y) / 2);
        
        HP_board.transform.position = new Vector2(transform.position.x + (spr.bounds.size.x - HP_board.GetComponent<SpriteRenderer>().bounds.size.x) / 2,
            transform.position.y - (spr.bounds.size.y - HP_board.GetComponent<SpriteRenderer>().bounds.size.y) / 2);

        Describe_board.transform.position = new Vector2(transform.position.x,
            transform.position.y - (spr.bounds.size.y - Describe_board.GetComponent<SpriteRenderer>().bounds.size.y - ATK_board.GetComponent<SpriteRenderer>().bounds.size.y * 2) / 2);
    }
    private void Boom()
    {
        //销毁卡牌，文本底板和文本模块
        Debug.Log("I am" + card_id + " I 'm beeing destroyed soon");
        Destroy(canv);
        Destroy(gameObject);
    }
    public void State_init()
    {
        //信号初始化
        attacked = false;
        back = true;
        dead = false;
        defense = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //碰撞时执行，结算伤害
        sb = 1000;
        Card_controller enemy = collision.gameObject.GetComponent<Card_controller>();//获取敌人
        HP = HP - enemy.Gs_atk;//扣血
        if (!(HP > 0))
        {
            //如果血扣光，停止运动，设置死亡信号为1，等待销毁。
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            dead = true;
            Debug.Log("I am" + card_id + " I'll die");
        }
        else
        {
            //如果没死
            if (!defense && !back)
            {
                //如果本次战斗中本张卡牌为攻击方
                attacked = true;//标记为已攻击
                gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2((location.x - transform.position.x) / 2, (location.y - transform.position.y) / 2);//归位移动
                Debug.Log("I am" + card_id + " I have already attacked but not back");
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
        defense = false;
        Debug.Log("I am" + card_id + " I have fnishied my defense");
    }
    public void Interrupt(float time, bool forever)//中断，详见战斗系统类
    {
        pause = true;
        if (!forever)
            Invoke("Interrupt_end", time);
    }
    private void Interrupt_end()
    {
        pause = false;
    }
    //private成员的get,set方法
    public int Gs_type
    {
        get { return type; }
        set { type = value; }
    }
    public int Gs_id
    {
        get { return card_id; }
        set { card_id = value; }
    }
    public int Gs_order
    {
        get { return order; }
        set { order = value; }
    }
    public string Gs_describe
    {
        get { return Describe; }
        set { Describe = value; }
    }
    public int Gs_level
    {
        get { return Level; }
        set { Level = value; }
    }
    public int Gs_atk
    {
        get { return ATK; }
        set { ATK = value; }
    }
    public int Gs_hp
    {
        get { return HP; }
        set { HP = value; }
    }
    public bool Gs_defense
    {
        get { return defense; }
        set { defense = value; }
    }
    public bool Gs_attacked
    {
        get { return attacked; }
        set { attacked = value; }
    }
    public bool Gs_back
    {
        get { return back; }
        set { back = value; }
    }
    public bool Gs_dead
    {
        get { return dead; }
        set { dead = value; }
    }
    public bool Gs_destroy
    {
        get { return destroy; }
        set { destroy = value; }
    }
    public void Set_lct(Vector2 l)
    { this.location = new Vector2(l.x, l.y); }
    public Vector2 Get_lct()
    { return this.location; }
}

