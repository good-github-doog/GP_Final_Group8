using UnityEngine;
using System.Collections;
using TMPro;
[RequireComponent(typeof(Book))]
public class AutoFlip : MonoBehaviour {
    public FlipMode Mode;
    public float PageFlipTime = 1;
    public float TimeBetweenPages = 1;
    public float DelayBeforeStarting = 0;
    public bool AutoStartFlip=true;
    public Book ControledBook;
    public int AnimationFramesCount = 40;
    bool isFlipping = false;
    public GameManager gm;   // 直接拖進 Inspector
    public float textFadeDuration = 0.6f;   // ⭐ 文字淡入時間
    public TextMeshProUGUI[] pageTexts;
    public UnityEngine.UI.Image[] pageImages;


    // Use this for initialization
    void Start () {
        if (!ControledBook)
            ControledBook = GetComponent<Book>();
        if (AutoStartFlip)
            StartFlipping();
        ControledBook.OnFlip.AddListener(new UnityEngine.Events.UnityAction(PageFlipped));
	}

    public void AutoFlipToPage(int targetPage)
    {
        StartCoroutine(AutoFlipTo(targetPage));
    }

    IEnumerator AutoFlipTo(int targetPage)
    {
        yield return new WaitForEndOfFrame(); // 等 Book 初始化完成
        HidePageText();

        if (targetPage > ControledBook.currentPage)
        {
            while (ControledBook.currentPage < targetPage)
            {
                FlipRightPage();
                yield return new WaitForSeconds(PageFlipTime + TimeBetweenPages);
            }
        }
        else if (targetPage < ControledBook.currentPage)
        {
            while (ControledBook.currentPage > targetPage)
            {
                FlipLeftPage();
                yield return new WaitForSeconds(PageFlipTime + TimeBetweenPages);
            }
        }
        yield return StartCoroutine(FadeInPageText());  // ⭐ 開始淡入

        if (gm.FirstTimeInGame)
        {
            Time.timeScale = 0f;
            gm.FirstTimeInGame = false;
        }
        
    }

    void HidePageText()
    {
        foreach (var t in pageTexts)
        {
            if (t == null) continue;
            var c = t.color;
            c.a = 0;
            t.color = c;
        }

        foreach (var img in pageImages)
        {
            if (img == null) continue;
            var c = img.color;
            c.a = 0;
            img.color = c;
        }
    }


    // ============================
    // ⭐ 文字淡入效果
    // ============================
    IEnumerator FadeInPageText()
    {
        float t = 0;
        while (t < textFadeDuration)
        {
            t += Time.deltaTime;
            float alpha = t / textFadeDuration;

            // Debug 看透明度
            // Debug.Log("Current Alpha = " + alpha.ToString("F2"));

            // ---- 淡入文字 ----
            foreach (var txt in pageTexts)
            {
                if (txt == null) continue;
                var c = txt.color;
                c.a = alpha;
                txt.color = c;
            }

            // ---- 淡入圖片 ----
            foreach (var img in pageImages)
            {
                if (img == null) continue;
                var c = img.color;
                c.a = alpha;
                img.color = c;
            }

            yield return null;
        }
    }

    void PageFlipped()
    {
        isFlipping = false;
    }
	public void StartFlipping()
    {
        StartCoroutine(FlipToEnd());
    }
    public void FlipRightPage()
    {
        if (isFlipping) return;
        if (ControledBook.currentPage >= ControledBook.TotalPageCount) return;
        isFlipping = true;
        float frameTime = PageFlipTime / AnimationFramesCount;
        float xc = (ControledBook.EndBottomRight.x + ControledBook.EndBottomLeft.x) / 2;
        float xl = ((ControledBook.EndBottomRight.x - ControledBook.EndBottomLeft.x) / 2) * 0.9f;
        //float h =  ControledBook.Height * 0.5f;
        float h = Mathf.Abs(ControledBook.EndBottomRight.y) * 0.9f;
        float dx = (xl)*2 / AnimationFramesCount;
        StartCoroutine(FlipRTL(xc, xl, h, frameTime, dx));
    }
    public void FlipLeftPage()
    {
        if (isFlipping) return;
        if (ControledBook.currentPage <= 0) return;
        isFlipping = true;
        float frameTime = PageFlipTime / AnimationFramesCount;
        float xc = (ControledBook.EndBottomRight.x + ControledBook.EndBottomLeft.x) / 2;
        float xl = ((ControledBook.EndBottomRight.x - ControledBook.EndBottomLeft.x) / 2) * 0.9f;
        //float h =  ControledBook.Height * 0.5f;
        float h = Mathf.Abs(ControledBook.EndBottomRight.y) * 0.9f;
        float dx = (xl) * 2 / AnimationFramesCount;
        StartCoroutine(FlipLTR(xc, xl, h, frameTime, dx));
    }
    IEnumerator FlipToEnd()
    {
        yield return new WaitForSeconds(DelayBeforeStarting);
        float frameTime = PageFlipTime / AnimationFramesCount;
        float xc = (ControledBook.EndBottomRight.x + ControledBook.EndBottomLeft.x) / 2;
        float xl = ((ControledBook.EndBottomRight.x - ControledBook.EndBottomLeft.x) / 2)*0.9f;
        //float h =  ControledBook.Height * 0.5f;
        float h = Mathf.Abs(ControledBook.EndBottomRight.y)*0.9f;
        //y=-(h/(xl)^2)*(x-xc)^2          
        //               y         
        //               |          
        //               |          
        //               |          
        //_______________|_________________x         
        //              o|o             |
        //           o   |   o          |
        //         o     |     o        | h
        //        o      |      o       |
        //       o------xc-------o      -
        //               |<--xl-->
        //               |
        //               |
        float dx = (xl)*2 / AnimationFramesCount;
        switch (Mode)
        {
            case FlipMode.RightToLeft:
                while (ControledBook.currentPage < ControledBook.TotalPageCount)
                {
                    StartCoroutine(FlipRTL(xc, xl, h, frameTime, dx));
                    yield return new WaitForSeconds(TimeBetweenPages);
                }
                break;
            case FlipMode.LeftToRight:
                while (ControledBook.currentPage > 0)
                {
                    StartCoroutine(FlipLTR(xc, xl, h, frameTime, dx));
                    yield return new WaitForSeconds(TimeBetweenPages);
                }
                break;
        }
    }
    IEnumerator FlipRTL(float xc, float xl, float h, float frameTime, float dx)
    {
        float x = xc + xl;
        float y = (-h / (xl * xl)) * (x - xc) * (x - xc);

        ControledBook.DragRightPageToPoint(new Vector3(x, y, 0));
        for (int i = 0; i < AnimationFramesCount; i++)
        {
            y = (-h / (xl * xl)) * (x - xc) * (x - xc);
            ControledBook.UpdateBookRTLToPoint(new Vector3(x, y, 0));
            yield return new WaitForSeconds(frameTime);
            x -= dx;
        }
        ControledBook.ReleasePage();
    }
    IEnumerator FlipLTR(float xc, float xl, float h, float frameTime, float dx)
    {
        float x = xc - xl;
        float y = (-h / (xl * xl)) * (x - xc) * (x - xc);
        ControledBook.DragLeftPageToPoint(new Vector3(x, y, 0));
        for (int i = 0; i < AnimationFramesCount; i++)
        {
            y = (-h / (xl * xl)) * (x - xc) * (x - xc);
            ControledBook.UpdateBookLTRToPoint(new Vector3(x, y, 0));
            yield return new WaitForSeconds(frameTime);
            x += dx;
        }
        ControledBook.ReleasePage();
    }
}
