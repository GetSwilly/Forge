using System;

[Flags]
public enum InputType {
    Press = 1,
    Hold = 1 << 1,
}
