# Real-Time-Hair-Sim

This project allows the user to grab the base on a lock of curly hair and drag it around.

The physics model that's used to make these hairs is based on visualizations I saw of Merida's hair that Pixar made, with my own alterations. It uses the standard mass-spring system of straight hair and connects each spring segment together with another spring to create a curl. 
Each point is also tethered to a 'guide hair' that's invisible. It acts like a normal straight hair and is used to keep the curl from moving too sluggishly. 

This hairs is by no means perfect. their movements can be sluggish, there is a lot of noise in their movements, and the more you move them the more messy they get. I hope to fix these issues in further iterations. 
