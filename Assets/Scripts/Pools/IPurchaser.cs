using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPurchaser {

    bool CanAfford(Cost c);

    void Purchase(Cost c);


}
