# CIS536 Caustics

Final project implementation for CIS536 final project.

To run the demos, open the project in Unity.
You can open and run any scene in the Scenes folder.
The camera view is interactive; right click and drag or use WASD to move.

## External Assets Used

The external assets used in this project are as follows:

[UVWorld](https://github.com/nobnak/UVWorld) by Nakata Nobuyuki, licensed MIT.
Modified to use UV2 channel rather than UV1.

[KDTree](https://github.com/viliwonka/KDTree) by Vili Volčini, licensed MIT.

[Cornell Box (Water)](https://casual-effects.com/data/) by Morgan McGuire, licensed CC-BY-3.0.
Modified to add a bottom to the mesh, creating a sealed object instead of a surface.

## External Algorithms Used

Overall photon map algorithm comes from:

Jensen, H. W., & Christensen, N. J. (1995). Photon maps in bidirectional Monte Carlo ray tracing of complex objects. Computers & Graphics, 19(2), 215-224.

and

Jensen, H. W., & Christensen, N. J. (2000). A practical guide to global illumination using photon maps. SIGGRAPH 2000 Course Notes CD-ROM.

Some specific ray tracing details come from:

Shirly, P. (2020). Ray Tracing in One Weekend. [https://raytracing.github.io/books/RayTracingInOneWeekend.html](https://raytracing.github.io/books/RayTracingInOneWeekend.html)

## External Tools Used

Ring model and water modifications were created in [Blender](https://www.blender.org/).
