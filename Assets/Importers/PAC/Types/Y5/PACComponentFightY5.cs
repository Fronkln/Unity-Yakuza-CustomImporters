using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PACComponentFightY5 : PACComponentY5
{
    public List<PACComponentY5> Fighters = new List<PACComponentY5>();

    public override void PostPACRead(PACComponentY5[] entities)
    {
        base.PostPACRead(entities);

        if (BaseEntityData.Length > 1)
            throw new System.Exception("BaseEntityData more than 1 for fight we dont know how to handle this");

        var dat = BaseEntityData[0];

        int enemyCount = int.Parse(dat.KeyValues.GetValueForKey("enemy_count"));

        for (int i = 0; i < enemyCount; i++)
        {
            int uid = int.Parse(dat.KeyValues.GetValueForKey("enemy" + i + "_uid"));
            PACComponentY5 fighter = entities.FirstOrDefault(x => x.transform.name.Contains(uid.ToString("X"), System.StringComparison.OrdinalIgnoreCase));

            Fighters.Add(fighter);
        }
    }

    public override void PreprocessExport(PACComponentY5[] entities)
    {
        base.PreprocessExport(entities);

        var dat = BaseEntityData[0];

        //reset kvs before appending them in 100% correct order
        dat.KeyValues.RemoveKeyValue("enemy_count");
        dat.KeyValues.RemoveKeyValue("boss_ptr");
        dat.KeyValues.RemoveKeyValue("pad1");
        dat.KeyValues.RemoveKeyValue("pad2");
        dat.KeyValues.RemoveKeyValue("pad3");

        int enemKey = 0;

        while(true)
        {
            if(!dat.KeyValues.ContainsKey("enemy" + enemKey + "_uid"))
                break;

            dat.KeyValues.RemoveKeyValue("enemy" + enemKey + "_uid");
            dat.KeyValues.RemoveKeyValue("enemy" + enemKey + "_idx");
            enemKey++;
        }

        dat.KeyValues.SetKeyValue("enemy_count", Fighters.Count.ToString(), KeyValueType.Short);
        dat.KeyValues.SetKeyValue("boss_ptr", 0x2C.ToString(), KeyValueType.Int);
        dat.KeyValues.SetKeyValue("pad1", "0", KeyValueType.Int);
        dat.KeyValues.SetKeyValue("pad2", "0", KeyValueType.Int);
        dat.KeyValues.SetKeyValue("pad3", "0", KeyValueType.Int);

        int enemyIdx = 0;

        for(int i = 0; i < Fighters.Count; i++)
        {
            var fighter = Fighters[i];

            if (fighter == null)
                continue;

            int uid = int.Parse(fighter.transform.name, System.Globalization.NumberStyles.HexNumber);
            dat.KeyValues.SetKeyValue("enemy" + enemyIdx + "_uid", uid.ToString(), KeyValueType.Int);
            enemyIdx++;
        }


        enemyIdx = 0;

        for (int i = 0; i < Fighters.Count; i++)
        {
            var fighter = Fighters[i];

            if (fighter == null)
                continue;

            dat.KeyValues.SetKeyValue("enemy" + enemyIdx + "_idx", Array.IndexOf(entities, fighter).ToString(), KeyValueType.Short);
            enemyIdx++;
        }
    }
}
