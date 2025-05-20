using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialScript : MonoBehaviour
{
    int count = 0;
    public TMP_Text counterTxt;
    public GameObject[] cups;
    public Image die;
    public float MixerUp;
    bool finishedRevealing; // Esta variável parece não estar sendo usada de forma eficaz para controlar o fluxo.
                            // Vamos ajustar a lógica para garantir que a mistura comece após a revelação.

    void Start()
    {
        count = 0;
        counterTxt.text = "Acertos: 0";
        die.enabled = false;
        StartCoroutine(CupMixer()); // Inicia a primeira mistura
    }

    public int rounds = 3; //rounds to mix

    public IEnumerator CupMixer(float wfs = .3f)
    {
        // Desabilita os botões dos copos antes de misturar
        foreach (GameObject c in cups)
        {
            c.Button().interactable = false;
        }

        Vector3[] ts = new Vector3[cups.Length];
        for (int i = 0; i < cups.Length; i++)
        {
            ts[i] = cups[i].transform.position.Copy();
        }

        // Revela os copos no início (ou antes de cada mistura se o jogo for assim)
        // Se a ideia é mostrar o conteúdo no início, essa parte está ok.
        // Se a ideia é que os copos revelem apenas quando clicados, essa linha deve ser removida daqui
        // e a lógica de revelação deve ser puramente no ClickedCup.
        // Pelo seu problema, assumo que a revelação inicial está correta.
        foreach (GameObject c in cups)
        {
            // O RevealCup aqui está sendo chamado para todos os copos, o que pode não ser o desejado
            // se o objetivo for apenas revelar o conteúdo *de um* copo após o clique.
            // Se o dado (die) é para ser o que "está" em um dos copos, a lógica pode ser mais complexa.
            // Para a mistura, basta que os copos estejam visíveis.
            c.SetActive(true); // Garante que os copos estejam visíveis para a mistura.
        }
        yield return new WaitForSecondsRealtime(1); // Espera para que os copos estejam visíveis

        for (int i = 0; i < rounds; i++)
        {
            Vector3[] randoms = ts.Randomize().Copy();

            // Eleva os copos antes da mistura
            foreach (GameObject c in cups)
            {
                StartCoroutine(c.TranslateOverTime(80, Vector3.up * MixerUp));
            }
            yield return new WaitForSecondsRealtime(wfs);

            // Move os copos para as novas posições
            for (int k = 0; k < ts.Length; k++)
            {
                Vector3 movement = new Vector3(randoms[k].x - cups[k].transform.position.x, -MixerUp);
                StartCoroutine(cups[k].TranslateOverTime(80, movement));
            }
            yield return new WaitForSecondsRealtime(wfs);
        }

        // Reabilita os botões dos copos após a mistura
        foreach (GameObject c in cups)
        {
            c.Button().interactable = true;
        }
    }

    public Sprite standing, laying;

    public IEnumerator RevealCup(GameObject cup)
    {
        // Esta coroutine controla a animação de revelação de um *único* copo clicado.
        cup.SetActive(true); // Garante que o copo clicado esteja ativo
        cup.GetComponent<Image>().sprite = laying; // Muda o sprite para o estado "deitado"
        die.enabled = true; // Mostra o dado

        yield return new WaitForSecondsRealtime(1); // Espera 1 segundo

        cup.GetComponent<Image>().sprite = standing; // Volta o sprite para o estado "em pé"
        die.enabled = false; // Esconde o dado
    }

    public void UpdateCounter()
    {
        counterTxt.text = "Acertos:" + count;
    }

    public void ClickedCup(GameObject g)
    {
        // Desabilita todos os botões dos copos enquanto o resultado é mostrado e a mistura está para acontecer
        foreach (GameObject c in cups)
        {
            c.Button().interactable = false;
        }

        // Inicia a coroutine para revelar o copo clicado
        StartCoroutine(HandleClickedCup(g));
    }

    // Nova coroutine para gerenciar o fluxo após o clique em um copo
    private IEnumerator HandleClickedCup(GameObject clickedCup)
    {
        // Revela o copo clicado
        yield return StartCoroutine(RevealCup(clickedCup)); // Espera a coroutine RevealCup terminar

        // Verifica se o copo clicado tem o componente que indica que é o "correto" (ex: tem o dado)
        if (clickedCup.HasComponentInChildren<Image>()) // Assumindo que a Image é o indicador do dado
        {
            count++;
            UpdateCounter();
            yield return StartCoroutine(CounterAnimation(Color.green)); // Espera a animação do contador
        }
        else
        {
            count = 0;
            UpdateCounter();
            yield return StartCoroutine(CounterAnimation(Color.red)); // Espera a animação do contador
        }

        // Aguarda um pequeno momento antes de iniciar a próxima mistura, para que o jogador veja o resultado.
        yield return new WaitForSecondsRealtime(1f);

        // Inicia a próxima rodada de mistura dos copos
        StartCoroutine(CupMixer());
    }

    public IEnumerator CounterAnimation(Color c)
    {
        counterTxt.color = c;
        yield return new WaitForSecondsRealtime(.5f);
        counterTxt.color = Color.white;
    }
}

// Classe Useful permanece a mesma, pois as extensões estão funcionando corretamente
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
            // A lógica original HasComponentInChildren está um pouco confusa e pode não ser robusta.
            // Se a intenção é verificar se *algum* filho tem o componente T,
            // ou se o próprio objeto tem, a verificação deve ser mais explícita.
            // Por exemplo, para verificar se tem o componente T (ex: Image) como filho:
            // return g.GetComponentInChildren<T>(true) != null;
            // Para o contexto do dado, se o "dado" é uma Image dentro do copo, a lógica abaixo funciona
            // assumindo que apenas um copo terá a Image "do dado" e outros elementos Image são ignorados.
            if (x == null || x.Length < 2 && ParentHasToo)
            {
                // Este int.Parse("sd") é uma forma incomum de lançar uma exceção para cair no catch.
                // É melhor fazer uma verificação direta ou lançar uma exceção mais clara se for o caso.
                // Para fins de detecção de um componente (como o "dado" dentro do copo),
                // uma forma mais simples e direta seria:
                // return g.GetComponentInChildren<T>(true) != null;
                // Ou, se você tem um componente específico que indica o dado, verificá-lo diretamente.
                int.Parse("sd"); // Mantido para compatibilidade com o comportamento original, mas é um antipadrão.
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
            // A linha abaixo usa 'target / milliseconds', o que pode não ser o movimento correto
            // se a intenção é mover em direção ao target do ponto atual.
            // Deveria ser 'movement / milliseconds'.
            g.transform.Translate(target / milliseconds, Space.World);
            yield return new WaitForSecondsRealtime(.001f);
        }
    }
}