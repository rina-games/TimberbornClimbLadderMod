#!/usr/bin/env python3
"""Write a 1x1 ladder .timbermesh. No Blender. Materials are existing game wood."""
import struct
import zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Buildings" / "Paths" / "Rope"


def varint(n):
    if n < 0:
        n += 1 << 64
    out = bytearray()
    while True:
        byte = n & 0x7F
        n >>= 7
        if n:
            out.append(byte | 0x80)
        else:
            out.append(byte)
            return bytes(out)


def key(field, wire):
    return varint((field << 3) | wire)


def field_varint(field, n):
    return key(field, 0) + varint(n)


def field_bytes(field, payload):
    return key(field, 2) + varint(len(payload)) + payload


def field_float(field, value):
    return key(field, 5) + struct.pack("<f", value)


def vec3(x, y, z):
    return field_float(1, x) + field_float(2, y) + field_float(3, z)


def quat_identity():
    return field_float(4, 1.0)


def vertex_property(name, dimension, floats):
    data = struct.pack("<" + "f" * len(floats), *floats)
    body = field_bytes(1, name.encode()) + field_varint(2, 4) + field_varint(3, dimension) + field_bytes(4, data)
    return field_bytes(7, body)


def mesh_message(indices, material):
    packed = b"".join(varint(i) for i in indices)
    body = field_bytes(1, packed) + field_bytes(2, material.encode())
    return field_bytes(8, body)


def add_face(verts, normals, uvs, tangents, indices, corners, normal, tangent):
    start = len(verts)
    for i, corner in enumerate(corners):
        verts.append(corner)
        normals.append(normal)
        uvs.append((0.0 if i in (0, 3) else 1.0, 0.0 if i in (0, 1) else 1.0))
        tangents.append(tangent)
    indices.extend((start, start + 1, start + 2, start, start + 2, start + 3))


def add_box(verts, normals, uvs, tangents, indices, x0, y0, z0, x1, y1, z1):
    # Unity front faces are clockwise when looking at the face from outside.
    add_face(verts, normals, uvs, tangents, indices,
             [(x1, y0, z1), (x1, y0, z0), (x1, y1, z0), (x1, y1, z1)], (1, 0, 0), (0, 0, -1, 1))
    add_face(verts, normals, uvs, tangents, indices,
             [(x0, y0, z0), (x0, y0, z1), (x0, y1, z1), (x0, y1, z0)], (-1, 0, 0), (0, 0, 1, 1))
    add_face(verts, normals, uvs, tangents, indices,
             [(x0, y1, z1), (x1, y1, z1), (x1, y1, z0), (x0, y1, z0)], (0, 1, 0), (1, 0, 0, 1))
    add_face(verts, normals, uvs, tangents, indices,
             [(x0, y0, z0), (x1, y0, z0), (x1, y0, z1), (x0, y0, z1)], (0, -1, 0), (1, 0, 0, 1))
    add_face(verts, normals, uvs, tangents, indices,
             [(x0, y0, z1), (x1, y0, z1), (x1, y1, z1), (x0, y1, z1)], (0, 0, 1), (1, 0, 0, 1))
    add_face(verts, normals, uvs, tangents, indices,
             [(x1, y0, z0), (x0, y0, z0), (x0, y1, z0), (x1, y1, z0)], (0, 0, -1), (-1, 0, 0, 1))


def ladder_geometry():
    verts, normals, uvs, tangents, indices = [], [], [], [], []
    # Two rails on the front of the cell. Y is up.
    add_box(verts, normals, uvs, tangents, indices, 0.16, 0.0, 0.08, 0.28, 1.0, 0.22)
    add_box(verts, normals, uvs, tangents, indices, 0.72, 0.0, 0.08, 0.84, 1.0, 0.22)
    # Lowest rung sat through a beaver's body. Keep the upper three.
    for y in (0.40, 0.62, 0.84):
        add_box(verts, normals, uvs, tangents, indices, 0.22, y, 0.10, 0.78, y + 0.07, 0.20)
    return verts, normals, uvs, tangents, indices


def flatten(values, width):
    out = []
    for item in values:
        out.extend(item[:width])
    return out


def build(material, model_name):
    verts, normals, uvs, tangents, indices = ladder_geometry()
    count = len(verts)
    node = b"".join([
        field_varint(1, -1),
        field_bytes(2, b"Ladder"),
        field_bytes(4, quat_identity()),
        field_bytes(5, vec3(1, 1, 1)),
        field_varint(6, count),
        vertex_property("position", 3, flatten(verts, 3)),
        vertex_property("normal", 3, flatten(normals, 3)),
        vertex_property("tangent", 4, flatten(tangents, 4)),
        vertex_property("uv0", 2, flatten(uvs, 2)),
        mesh_message(indices, material),
    ])
    model = field_varint(1, 1) + field_bytes(2, model_name.encode()) + field_bytes(3, node)
    compressor = zlib.compressobj(level=6, wbits=15)
    return compressor.compress(model) + compressor.flush()


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    files = {
        "RopeLadder.Folktails.Model.timbermesh": "BaseWood_Brown.Folktails",
        "RopeLadder.IronTeeth.Model.timbermesh": "BaseWood_DarkBrown.IronTeeth",
    }
    for filename, material in files.items():
        payload = build(material, filename.replace(".timbermesh", ""))
        path = OUT / filename
        path.write_bytes(payload)
        print(path, len(payload), "header", payload[:2].hex())


if __name__ == "__main__":
    main()
