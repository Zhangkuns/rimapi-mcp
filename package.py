#!/usr/bin/env python3
"""Create a standalone MCP folder with API reference; never copy credentials or venvs."""
import argparse
from pathlib import Path
import shutil

from rimapi_server import HERE, References


def package(destination: Path):
    destination.mkdir(parents=True, exist_ok=True)
    for name in ["rimapi_server.py", "smoke.py", "test_rimapi_mcp.py", "package.py", "requirements.txt",
                 ".env.example", ".gitignore", "README.md", "PLAYING.md"]:
        shutil.copy2(HERE / name, destination / name)
    license_path = HERE / "LICENSE" if (HERE / "LICENSE").is_file() else HERE.parent / "LICENSE"
    shutil.copy2(license_path, destination / "LICENSE")
    refs = References()
    for name, source in refs.files.items():
        target = destination / "references" / name
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source, target)
    print(f"Packaged {len(refs.files)} API reference files into {destination}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("destination", type=Path)
    package(parser.parse_args().destination.resolve())
