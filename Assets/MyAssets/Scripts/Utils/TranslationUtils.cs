using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class TranslationUtils : MonoBehaviour
{
    /**
     * Returns string for a key from Localization table.
     */
    public static string Translate(string key)
    {
        var op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("UI Text", key); // TODO: change table name
        string translation = "";
        if (op.IsDone)
            translation = op.Result;
        else
            op.Completed += (o) => translation = o.Result;

        return translation;
    }
}
