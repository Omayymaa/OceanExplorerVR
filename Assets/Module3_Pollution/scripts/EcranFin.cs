using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EcranFin : MonoBehaviour
{
    public static EcranFin Instance;
    public GameObject panelFin;
    public TMP_Text texteFin;
    public int objectif = 10;      // déchets à ramasser pour finir

    int ramasses;

    void Awake() => Instance = this;

    public void DechetRamasse()
    {
        ramasses++;
        if (ramasses >= objectif) Afficher();
    }

    void Afficher()
    {
        panelFin.SetActive(true);
        texteFin.text =
            "Bravo ! Vous avez ramassé " + ramasses + " déchets.\n\n" +
            "Un seul sac plastique peut mettre jusqu'à 400 ans à se dégrader.\n" +
            "Chaque déchet retiré protège les poissons, les tortues et les coraux.\n" +
            "Trier, c'est agir pour l'océan.";
        Time.timeScale = 0f;   // fige le jeu
    }

    public void Rejouer()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}