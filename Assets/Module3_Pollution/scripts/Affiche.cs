using UnityEngine;
using TMPro;

public class Affiche : MonoBehaviour
{
    public static Affiche Instance;

    public TMP_Text texte;
    public int maxDechets = 15;
    public float intervalle = 1f;
    public float dureeMinimale = 8f;
    public float dureeFait = 6f;

    int deposes;
    string fait = "";
    float finFait;
    int niveauActuel = -1;
    float debutNiveau;
    int dernierFait = -1;

    string[] faitsEducatifs =
    {
        "Une bouteille en plastique met environ 450 ans à se dégrader.",
        "Le plastique se fragmente en microplastiques avalés par les poissons.",
        "Les tortues confondent souvent les sacs plastique avec des méduses.",
        "Trier ses déchets protège la vie marine.",
        "Une grande partie des déchets marins vient de la terre ferme."
    };

    void Awake() => Instance = this;

    void Start() => InvokeRepeating(nameof(Actualiser), 0f, intervalle);

    public void DechetDepose()
    {
        deposes++;

        int i;
        do { i = Random.Range(0, faitsEducatifs.Length); }
        while (i == dernierFait && faitsEducatifs.Length > 1);
        dernierFait = i;

        fait = faitsEducatifs[i];
        finFait = Time.time + dureeFait;
        niveauActuel = -1;
        Actualiser();
    }

    void Actualiser()
    {
        if (texte == null) return;

        if (Time.time < finFait)
        {
            texte.text = "<b>Le saviez-vous ?</b>\n" + fait;
            texte.color = new Color(0.05f, 0.35f, 0.15f);
            return;
        }

        int n = GameObject.FindGameObjectsWithTag("Trash").Length;
        float p = Mathf.Clamp01((float)n / maxDechets);
        int voulu = p < 0.2f ? 0 : p < 0.5f ? 1 : p < 0.8f ? 2 : 3;

        if (niveauActuel != -1 && voulu != niveauActuel && Time.time - debutNiveau < dureeMinimale)
            voulu = niveauActuel;

        if (voulu != niveauActuel)
        {
            niveauActuel = voulu;
            debutNiveau = Time.time;
        }

        switch (niveauActuel)
        {
            case 0:
                texte.text = "<b>Océan presque propre !</b>\nDéchets ramassés : " + deposes;
                texte.color = new Color(0.05f, 0.35f, 0.15f);
                break;
            case 1:
                texte.text = "<b>Attention</b>\n" + n + " déchets menacent la vie marine.";
                texte.color = new Color(0.6f, 0.4f, 0f);
                break;
            case 2:
                texte.text = "<b>Pollution élevée !</b>\n" + n + " déchets dans l'eau.\nLe robot a besoin d'aide.";
                texte.color = new Color(0.8f, 0.3f, 0f);
                break;
            default:
                texte.text = "<b>ALERTE !</b>\nL'océan est saturé de déchets (" + n + ").\nLes poissons sont en danger.";
                texte.color = new Color(0.75f, 0.1f, 0.1f);
                break;
        }
    }
}