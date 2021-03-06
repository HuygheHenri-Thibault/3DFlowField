# 3D Flowfield
 DAE 2020-2021 Gameplay Programming exam research topic.
 
## Research Purpose
 My interest in RTS games caused me to pick this topic, during class I had heard flowfields are often used to handle pathfinding for RTS games. Most flowfields found during researching the topic seemed to be 2D implementations which made me wonder how a 3D implementation would work.
 
 I made my implementation with the intent of finding out whether 3D flowfields are even practical, what they could be useful for and if making it 3D would cause any issues.
 
## What is a flowfield?
 A Flowfield in essence, is a way of getting a direction towards some destination inside of the field from every other posistion in the field. To do this, a flowfield splits up the world into cells which, after some calculation will contain a vector which is the direction an agent should go if it finds itself in that cell to go towards the destination.

## What are flowfields commonly used for?
 Flowfields in games are most commonly used for efficient crowd pathfinding, situations where a large group of units/agents need to move towards a common destination. 
 ![RTS example](https://tyskwo.com/assets/images/portfolio/2015/flow-field-pathfinding/feature.png)  
 *Source: https://tyskwo.com/work/91_flow-field-pathfinding/*
 
 They can also be used in racing games, where the flow field points towards the best path to take on a race track. Flowfields can also be used to generate curves which could then be generated and displayed visually as art.
 ![Art 1](https://images.squarespace-cdn.com/content/v1/5c12933f365f02733c923e4e/1580788110303-VLBVW9I9EHKF4APSFZUK/ke17ZwdGBToddI8pDm48kLPswmMOqQZ9-Q6KHLjvbpZ7gQa3H78H3Y0txjaiv_0fDoOvxcdMmMKkDsyUqMSsMWxHk725yiiHCCLfrh8O1z5QPOohDIaIeljMHgDF5CVlOqpeNLcJ80NK65_fV7S1UTcpTqfU-ZEsztPyQLxhSSK-PhJjRDDFQG0l3_ZnmWi1QjT9byXZM3ISxo3y1NRptg/long-curves.jpg?format=750w)
 ![Art 2](https://images.squarespace-cdn.com/content/v1/5c12933f365f02733c923e4e/1580788231770-ESX9MD7L5YN3DE7J7JCW/ke17ZwdGBToddI8pDm48kEY24XRp8jv9M12bso-nFLUUqsxRUqqbr1mOJYKfIPR7LoDQ9mXPOjoJoqy81S2I8N_N4V1vUb5AoIIIbLZhVYxCRW4BPu10St3TBAUQYVKc6BM6kmiMikd-H1LQ4OOAeC1Z164ihj-qzUVOZU_yAZfy50s-qPI76RWVQvQuQiOr/unfenced-existence.png?format=750w)  
 *Source: https://tylerxhobbs.com/essays/2020/flow-fields*
 
 Dynamic Flowfields can also change on their own over time resulting in a different output for a single input, this is used to simulate fluid flows for instance. Lastly a Flowfield can also be used to simulate how sound would travel in a space, by setting the target of a Flowfield to the source of a sound and then inverting the vectors leading to it you can get the direction sound would travel in a space.
 
## My implementation
 As previously mentioned my Flowfield is a 3D Flowfield, it splits up the area it covers into equally sized cubes and generates their directions towards a target. Every cell has it's own such as (but not limited to): `worldPos`, `cost`, `bestDirection`. These cells are then contained inside of a Flowfield class which handles all the heavy lifting when it comes to generation of the field, cost calculation and direction calculation. It should also be noted that my implementation dynamically checks for collisions with objects marked as impassible & rough terrain, cells which become impassible are given the highest cost a cell can have and ignored when calculating the `bestCost` of cells and their `bestDirection`, impassible cells have no direction. Cells marked as rough/difficult terrain have an increased base cost compared to other cells but are still used in the calculation of the `bestCost` and `bestDirection`.
 
 Then once the flowfield is generated and it's cell's directions have been calculated an `AgentController` spawns a set number of agents in which will follow the flowfield towards the target as a demonstation.
 
## Result
 The 3D flowfield works well and in the end is not too different from a 2D implementation. Most 2D things such as grid, vectors, positions & directions just become 3D versions and the cost calculations remain the same. One disadvantage is the significant amount of cells created due to the flowfield being in 3D space instead of 2D, every hight layer adds a significant amount of cells. For the current 'smaller' implementation some 12000 cells are made but performance is still good in debug mode.
 
## Conclusion
 3D Flowfields seem to have interesting uses when used in 3D space, such as sound simulation. Though for pathfinding of flying agents the large amount of cells in a bigger space could result in a large drop in performance. A flowfield using a quadtree implementation might be able to relieve some of this strain, since it could have much larger cells for big open spaces. But at this time I do not know if such a thing would even be possible in a flowfield.
 Simulation of sound using flowfields has caught my interest however and sound simulation in a 3D space could be interesting to research next.
 
## Controls
 The Unity project (Unity 2019.4.17f1) in this repository has some controls which are listed below:
 ### For AgentController.cs  
 `R`, Sets a new random target for the agents to navigate towards  
 `Up Arrow`, Spawns another set of agents in random positions on the field (The amount can be changed in the editor)  
 `Down Arrow`, Destroys all active agents  
 
 ### For Flowfield.cs  
 `V`, toggles display of cell's their direction vectors **Warning! Somewhat laggy**  
 `B`, toggles display of rough (yellow) and impassible (red) cells
 