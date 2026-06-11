using System;

[Flags]
public enum AbilityActivationTrigger
{
    None = 0,
    SlotOne = 1 << 0,
    SlotTwo = 1 << 1,
    SlotThree = 1 << 2
}
