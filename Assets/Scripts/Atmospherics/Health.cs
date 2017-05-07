using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {
    public List<BodyPart> bodyParts = new List<BodyPart>();

	void Start () {
		
	}
	
	void Update () {
		
	}
}

public class BodyPart {
    private int oxygenDamage;
    private int burnDamage;
    private int bluntDamage;

    private List<Effects> effects = new List<Effects>();
    private int health;

    public void changeOxygenDamage(int change) {
        oxygenDamage += change;
        if(oxygenDamage > 100) {
            addEffect(Effects.Dead);
            oxygenDamage = 100;
        }
        if(oxygenDamage < 0) {
            oxygenDamage = 0;
        }
    }
    public void changeBurnDamage(int change) {
        burnDamage += change;
        if(burnDamage > 100) {
            addEffect(Effects.Dead);
            burnDamage = 100;
        }
        if(burnDamage < 0) {
            burnDamage = 0;
        }
    }
    public void changeBluntDamage(int change) {
        bluntDamage += change;
        if(bluntDamage > 100) {
            addEffect(Effects.Dead);
            bluntDamage = 100;
        }
        if(bluntDamage < 0) {
            bluntDamage = 0;
        }
    }



    public int returnHealth() {
        return health - oxygenDamage - burnDamage - bluntDamage;
    }

    public bool addEffect(Effects _effect) {
        if(hasEffect(_effect)) {
            return false;
        }
        else {
            effects.Add(_effect);
            return true;
        }
    }

    public bool removeEffect(Effects _effect) {
        if(hasEffect(_effect)) {
            return false;
        }
        else {
            effects.Remove(_effect);
            return true;
        }
    }

    public bool hasEffect(Effects _effect) {
        bool isAffectedBy = false;
        foreach(Effects i in effects) {
            if(i == _effect) {
                isAffectedBy = true;
            }
        }
        return isAffectedBy;
    }
}

public enum Effects {
    Dead
}