# ICFPC 2016. Team kontur.ru (ID: 89)

## Members

* Alexander Borzunov — infrastructure, one of automatic solvers, manual solver
* Ivan Dashkevich — convex solver, infrastructure, birthday!!!
* Ivan Domashnikh — solver
* Alexey Dubrovin — infrastructure, solver
* Pavel Egorov — visualizer, manual solver, problems, this readme
* Mikhail Khruschev — constructor solver
* Alexey Kirpichnikov — help everybody
* Alexandr Kokovin — problems
* Andrey Kostousov — convex solver, infrastructure
* Grigoriy Nazarov — help everybody
* Yuriy Okulovskiy — solver
* Dmitriy Titarenko — problems, infrastructure

## Notable places in the source code

Solvers:

* [lib/ConvexPolygonSolver.cs](lib/ConvexPolygonSolver.cs) — for convex problems
* [lib/SolutionSpecExt.cs](lib/SolutionSpecExt.cs) — implementation of the solution folding
* [lib/ProjectionSolver/UltraSolver.cs](lib/ProjectionSolver/UltraSolver.cs) — for simple non-convex problems (tries to compose initial square perimeter with rational segments from a problem and then restore other points)
* [lib/Constructor/ConstructorSolver.cs](lib/Constructor/ConstructorSolver.cs) — for other simple non-convex problems (tries to construct the initial square from polygons of the given silhouette)
* [lib/Visualization/ManualSolving/ManualSolverForm.cs](lib/Visualization/ManualSolving/ManualSolverForm.cs) — UI for manual problem solving (you can do several reflections and then apply the convex solver)
* [lib/Visualization/Visualizer.cs](lib/Visualization/Visualizer.cs) — visualizer of all problems with ability to open ManualSolverForm
* [lib/SolutionPacker.cs](lib/SolutionPacker.cs) — optimizer of the solution size

Problem generators:

* [lib/D4Problem.cs](lib/D4Problem.cs) — generator of problems not solvable in 3 dimensions.
* [lib/Problems.cs](lib/Problems.cs) — generator of some other simple problems.

Infrastructure:

* [TimeManager/](TimeManager/) — throttler for api calls
* [AutoSolver/](AutoSolver/) — daemon that downloads new problems and runs solvers.
* [problems/](problems/) — directory with problems, solutions and responses of solution/submit api call

## Manual solver?!?

Yes, we gain a lot of points via a semi-automatic solver. A human needs to do several folds to make a figure convex, and then the convex solver do the rest.

<a href="http://www.youtube.com/watch?feature=player_embedded&v=6_2JAzxuTNM
" target="_blank"><img src="http://img.youtube.com/vi/6_2JAzxuTNM/0.jpg" 
alt="IMAGE ALT TEXT HERE" width="240" height="180" border="10" /></a>
