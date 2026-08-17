INCLUDE ../../System/global_variables.ink


{
- irene_progress == 0:
    -> First_Encounter
- irene_progress == 1:
    -> Second_Encounter
- irene_progress == 2: 
    -> Third_Encounter
}


-> First_Encounter

=== First_Encounter ===
Hey there! #source:unknown #portrait:irene_happy
My name is Irene #source:known
What's yours?
Do you have a <color=\#edc966>name</color>? #portrait:irene_doubt
    * [Yes I do have one]
        What is it?
        ** [Yes]
            You don't make any sense!
            ~ irene_progress++
            -> END
        ** [No]
            Alright, don't tell me your name then #portrait:irene_sad
            ~ irene_progress++
            -> END
    * [No I do not have one my little princess]
        That's weird
        ~ irene_progress++
        -> END

=== Second_Encounter ===
#source:known #portrait:irene_doubt
You still haven't told me your <color=\#edc966>name</color> #portrait:irene_doubt
    * [Tell her your <color=\#edc966>name</color>]
        Oh that's a cute name! #portrait:irene_surprised
        ~ irene_progress++
        -> END
    * [Don't]
        Why won't you tell me your name?? #portrait:irene_sad
        ~ irene_progress++
        -> END

=== Third_Encounter ===
#source:known #portrait:irene_happy
- I have no more <color=\#cf75d9>dialogue</color>, let's meet again once I do! 
-> END
ortrait:irene_happy
-> END
