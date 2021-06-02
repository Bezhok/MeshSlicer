# MeshSlicer
Doesn't support submeshes.

For triangulationg "ear clipping with holes" algorithm is used. O(n^2) where n - slice points count.
Doesn't support self intersections slices.
Can cause some errors for an open shape (bad mesh).
Supports multi-hole polygons and shape hierarchy

# GIF
![gg](https://user-images.githubusercontent.com/30340548/120530991-c7fbe280-c3e6-11eb-89cc-e035ce851ea1.gif)
