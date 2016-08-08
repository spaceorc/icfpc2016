# ICFPC 2016. Team kontur.ru (ID: 89)

## Members

* Pavel Egorov — visualizer, manual solver, problems, this readme
* Mikhail Khruschev — solver
* Yuriy Okulovskiy — solver
* Ivan Dashkevich — convex solver, infrastructure, birthday!!!
* Andrey Kostousov— convex solver, infrastructure
* Dmitriy Titarenko — problems, infrastructure
* Alexandr Borozdin — infrastructure, manual solver, solver
* Alexey Dubrovin — infrastructure, solver
* Ivan Domashnikh — solver
* Alexey Kirpichnikov — help
* Grigoriy Nazarov — help
* Alexandr Kokovin — help


## Interesting classes in source:

Solvers:

* lib/ConvexPolygonSolver.cs — for convex problems
* lib/SolutionSpecExt.cs — implementation of the solution folding.
* lib/ProjectionSolver/UltraSolver.cs — for simple not convex problems (try to compose initial square perimeter with rational segments from problem and then restore other points)
* lib/ManualSolverForm.cs — UI for manual problems solving (you can do several reflections and then apply convex solver)
* lib/VisualizerForm.cs — visualizer of all problems with ability to open ManualSolverForm
* lib/SolutionPacker.cs — optimizer of the solution size.

Problems:

* lib/D4Problem.cs — generator of problems not solvable in 3 dimensions.
* lib/Problems.cs — generator of some other simple problems.

Infrastructure:

* TimeManager — throttler for api calls
* AutoSolver — daemon, download new problems and run solvers.
* problems/ — directory with problems, solutions and responses of solution/submit api call

## Manual solver?!?

<iframe width="560" height="315" src="https://www.youtube.com/embed/6_2JAzxuTNM" frameborder="0" allowfullscreen></iframe>

