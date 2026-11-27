using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Spanish names of the signs, worked out from the sprite sheet name
/// (for example "caracol_sheet" -> "Caracol", "que_hora_es_sheet" -> "¿Qué hora es?").
/// </summary>
public static class SignNames
{
    // Sheet names (without "_sheet") whose Spanish spelling needs accents or punctuation.
    private static readonly Dictionary<string, string> names = new Dictionary<string, string>
    {
        // Animales
        { "caballo", "Caballo" },
        { "caracol", "Caracol" },
        { "cerdo", "Cerdo" },
        { "conejo", "Conejo" },
        { "gallina", "Gallina" },
        { "gato", "Gato" },
        { "mariposa", "Mariposa" },
        { "oveja", "Oveja" },
        { "paloma", "Paloma" },
        { "pato", "Pato" },
        { "perro", "Perro" },
        { "tortuga", "Tortuga" },
        { "vaca", "Vaca" },

        // Estaciones
        { "primavera", "Primavera" },
        { "verano", "Verano" },
        { "otono", "Otoño" },
        { "invierno", "Invierno" },

        // Hora
        { "que_hora_es", "¿Qué hora es?" },
        { "1_hora", "1 hora" },
        { "5_minutos", "5 minutos" },
        { "10_segundos", "10 segundos" },
        { "3_15", "3:15" },
        { "7_30", "7:30" },
        { "9_45", "9:45" },
        { "12_00", "12:00" },

        // Transportes
        { "autobus", "Autobús" },
        { "avion", "Avión" },
        { "barco", "Barco" },
        { "bicicleta", "Bicicleta" },
        { "camion", "Camión" },
        { "coche", "Coche" },
        { "helicoptero", "Helicóptero" },
        { "moto", "Moto" },
        { "taxi", "Taxi" },
        { "tren", "Tren" },
    };

    /// <summary>Spanish name for a sign, from its sprite sheet (or, if missing, its first frame).</summary>
    public static string FromSheet(Texture2D sheet, Sprite[] frames)
    {
        if (sheet != null)
            return FromSheetName(sheet.name);

        if (frames != null && frames.Length > 0 && frames[0] != null)
        {
            // Frame names look like "caracol_sheet_12": drop the frame number.
            string frameName = frames[0].name;
            string withoutNumber = Regex.Replace(frameName, @"_\d+$", "");
            return FromSheetName(withoutNumber.EndsWith("_sheet") ? withoutNumber : frameName);
        }

        return "";
    }

    /// <summary>Spanish name for a sheet name such as "caracol_sheet" or "que_hora_es".</summary>
    public static string FromSheetName(string sheetName)
    {
        if (string.IsNullOrWhiteSpace(sheetName)) return "";

        string key = sheetName.Trim().ToLowerInvariant();
        if (key.EndsWith("_sheet"))
            key = key.Substring(0, key.Length - "_sheet".Length);

        if (names.TryGetValue(key, out string name))
            return name;

        // Unknown sheet: "mi_signo" -> "Mi signo".
        string text = key.Replace('_', ' ');
        return text.Length == 0 ? "" : char.ToUpperInvariant(text[0]) + text.Substring(1);
    }
}
