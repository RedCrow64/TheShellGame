using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialScript : MonoBehaviour
{
    int count = 0;
    public TMP_Text counterTxt;
    public TMP_Text highScoreTxt; // Adicionado para exibir o recorde
    private int highScore = 0; // Adicionado para armazenar o recorde

    public GameObject[] cups;
    public Image die;
    public float MixerUpPercentage = 0.1f; // Eleva os copos em 10% da altura da tela
    public float MixerSideRangePercentage = 0.3f; // NOVO: Define a amplitude lateral de mistura (ex: 30% da largura da tela)

    // Array para armazenar as posições de destino fixas dos copos.
    // VOCÊ AINDA PRECISA DEFINIR ESSAS POSIÇÕES NO INSPECTOR!
    public Vector3[] fixedCupPositions;

    void Start()
    {
        count = 0;
        counterTxt.text = "Counter: 0";
        die.enabled = false;

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateHighScoreText();

        // NOVO: Verificar e inicializar fixedCupPositions
        if (fixedCupPositions == null || fixedCupPositions.Length != cups.Length)
        {
            Debug.LogError("As posições fixas dos copos não foram definidas corretamente no Inspector! " +
                           "Por favor, defina um array de Vector3 com o mesmo número de copos.");
            // Fallback: usar as posições atuais dos copos como fixas
            fixedCupPositions = new Vector3[cups.Length];
            for (int i = 0; i < cups.Length; i++)
            {
                fixedCupPositions[i] = cups[i].transform.position;
            }
        }

        // NOVO: Mover os copos para as posições fixas antes da primeira mistura
        // Garante que eles comecem no lugar certo
        for (int i = 0; i < cups.Length; i++)
        {
            cups[i].transform.position = fixedCupPositions[i];
            cups[i].SetActive(true); // Garante que estejam visíveis
        }

        StartCoroutine(CupMixer()); // Inicia a primeira mistura
    }

    public int rounds = 3; // rounds to mix

    public IEnumerator CupMixer(float wfs = .3f)
    {
        foreach (GameObject c in cups)
        {
            c.Button().interactable = false;
        }

        // NOVO: Use fixedCupPositions como a base para o embaralhamento
        // Isso garante que o embaralhamento dinâmico ocorra a partir de um ponto conhecido.
        Vector3[] basePositions = fixedCupPositions.Copy(); // Copia as posições fixas

        yield return new WaitForSecondsRealtime(1); // Espera para que os copos estejam visíveis

        float actualMixerUpHeight = Screen.height * MixerUpPercentage;
        float actualMixerSideRange = Screen.width * MixerSideRangePercentage; // NOVO: Largura de movimento lateral

        for (int i = 0; i < rounds; i++)
        {
            // Gera um novo conjunto de posições randomizadas baseadas nas posições fixas
            Vector3[] randoms = basePositions.Randomize().Copy();

            // ELEVAR E MOVER LATERALMENTE:
            List<Coroutine> initialMoveCoroutines = new List<Coroutine>();
            for (int k = 0; k < cups.Length; k++)
            {
                // Calcula o movimento lateral relativo à nova posição randomizada
                // (randoms[k].x - cups[k].transform.position.x) seria o movimento lateral.
                // Mas queremos que eles se movam para cima e depois para as posições randomizadas.
                // Vamos usar a segunda sobrecarga de TranslateOverTime para ir para um alvo:
                Vector3 targetAboveRandom = new Vector3(randoms[k].x, randoms[k].y + actualMixerUpHeight, randoms[k].z);
                initialMoveCoroutines.Add(StartCoroutine(cups[k].TranslateOverTime(targetAboveRandom, 30)));
            }
            // Espera todos os movimentos iniciais (elevar e ir para a posição randômica no ar)
            foreach (Coroutine coroutine in initialMoveCoroutines)
            {
                yield return coroutine;
            }
            yield return new WaitForSecondsRealtime(wfs); // Espera um pouco no topo

            // DESCER:
            List<Coroutine> finalMoveCoroutines = new List<Coroutine>();
            for (int k = 0; k < cups.Length; k++)
            {
                // Move os copos para as posições randomizadas no chão (descendo)
                finalMoveCoroutines.Add(StartCoroutine(cups[k].TranslateOverTime(randoms[k], 30)));
            }
            // Espera todos os movimentos de descida
            foreach (Coroutine coroutine in finalMoveCoroutines)
            {
                yield return coroutine;
            }
            yield return new WaitForSecondsRealtime(wfs);
        }

        // ETAPA FINAL: Animar os copos de volta para as posições fixas e predefinidas (1, 2, 3)
        // Isso garante que, no final do embaralhamento, eles estejam sempre nas posições visíveis e selecionáveis.
        yield return StartCoroutine(MoveCupsToFixedPositions(fixedCupPositions, 30)); // 80ms para a animação de retorno

        foreach (GameObject c in cups)
        {
            c.Button().interactable = true;
        }
    }

    // Coroutine para mover os copos para posições fixas
    public IEnumerator MoveCupsToFixedPositions(Vector3[] targetPositions, int milliseconds)
    {
        List<Coroutine> moveCoroutines = new List<Coroutine>();
        for (int i = 0; i < cups.Length; i++)
        {
            moveCoroutines.Add(StartCoroutine(cups[i].TranslateOverTime(targetPositions[i], milliseconds)));
        }
        foreach (Coroutine coroutine in moveCoroutines)
        {
            yield return coroutine;
        }
    }

    public Sprite standing, laying;

    public IEnumerator RevealCup(GameObject cup)
    {
        cup.SetActive(true);
        cup.GetComponent<Image>().sprite = laying;
        die.enabled = true;

        yield return new WaitForSecondsRealtime(1);

        cup.GetComponent<Image>().sprite = standing;
        die.enabled = false;
    }

    public void UpdateCounter()
    {
        counterTxt.text = "Counter " + count;
    }

    private void UpdateHighScoreText()
    {
        highScoreTxt.text = "High Score: " + highScore;
    }

    public void ClickedCup(GameObject g)
    {
        foreach (GameObject c in cups)
        {
            c.Button().interactable = false;
        }
        StartCoroutine(HandleClickedCup(g));
    }

    private IEnumerator HandleClickedCup(GameObject clickedCup)
    {
        yield return StartCoroutine(RevealCup(clickedCup));

        if (clickedCup.HasComponentInChildren<Image>())
        {
            count++;
            UpdateCounter();

            if (count > highScore)
            {
                highScore = count;
                PlayerPrefs.SetInt("HighScore", highScore);
                UpdateHighScoreText();
            }

            yield return StartCoroutine(CounterAnimation(Color.green));
        }
        else
        {
            count = 0;
            UpdateCounter();
            yield return StartCoroutine(CounterAnimation(Color.red));
        }

        yield return new WaitForSecondsRealtime(1f);

        StartCoroutine(CupMixer());
    }

    public IEnumerator CounterAnimation(Color c)
    {
        counterTxt.color = c;
        yield return new WaitForSecondsRealtime(.5f);
        counterTxt.color = Color.white;
    }
}

// A classe Useful permanece a mesma, pois as correções já foram aplicadas
public static class Useful
{
    public static Button Button(this GameObject g) => g.GetComponent<Button>();
    public static Vector3[] Positions(this GameObject[] gs)
    {
        List<Vector3> vs = new List<Vector3>();
        foreach (GameObject g in gs)
        {
            Vector3 v = g.transform.position;
            vs.Add(new Vector3(v.x, v.y, v.z));
        }
        return vs.ToArray();
    }
    public static T[] Randomize<T>(this T[] arr)
    {
        List<T> ts = new List<T>();
        List<int> index = new List<int>(), ni = new List<int>();//ni- new indexes
        for (int i = 0; i < arr.Length; i++) index.Add(i);
        while (index.Count > 0)
        {
            int ind = Random.Range(0, index.Count);
            ni.Add(index[ind]);
            index.RemoveAt(ind);
        }
        foreach (int n in ni) ts.Add(arr[n]);
        return ts.ToArray();
    }
    public static bool HasComponentInChildren<T>(this GameObject g, bool ParentHasToo = true)
    {
        try
        {
            T[] x = g.GetComponentsInChildren<T>();
            if (x == null || x.Length < 2 && ParentHasToo)
            {
                int.Parse("sd");
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    public static Vector3 Copy(this Vector3 v) => new Vector3(v.x, v.y, v.z);
    public static Vector3[] Copy(this Vector3[] vs) { List<Vector3> nv = new List<Vector3>(); foreach (Vector3 v in vs) nv.Add(new Vector3(v.x, v.y, v.z)); return nv.ToArray(); }
    public static void SetActive(this GameObject[] gs, bool b) { foreach (GameObject g in gs) g.SetActive(b); }
    public static IEnumerator TranslateOverTime(this GameObject g, int milliseconds, Vector3 movement)
    {
        for (int i = 0; i < milliseconds; i++)
        {
            g.transform.Translate(movement / milliseconds, Space.World);
            yield return new WaitForSecondsRealtime(.001f);
        }
    }
    public static IEnumerator TranslateOverTime(this GameObject g, Vector3 target, int milliseconds)
    {
        Vector3 movement = target - g.transform.position;
        for (int i = 0; i < milliseconds; i++)
        {
            g.transform.Translate(movement / milliseconds, Space.World);
            yield return new WaitForSecondsRealtime(.001f);
        }
    }
}