INCLUDE ../../System/global_variables.ink

{
- gauss_progress == 0:
    -> First_Encounter
- gauss_progress == 1:
    -> Second_Encounter
}

=== First_Encounter ===
Fuck you #source:unknown #portrait:gauss_angry
name's Gauss #source:known
~ gauss_progress++
-> END

=== Second_Encounter ===
thought I told you to go fuck yourself #portrait:gauss_doubt
Don't pay attention to him, he's just mean #source:Irene #portrait:irene_sad
-> END