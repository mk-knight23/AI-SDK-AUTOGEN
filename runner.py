import argparse

try:
    from .app import run_autogen_mission
except ImportError:
    from app import run_autogen_mission


def demo(mission: str) -> None:
    out = run_autogen_mission(mission)
    print("[AutoGen] primary:", out.get("primary"))
    print("[AutoGen] support:", out.get("support"))
    print("[AutoGen] result:", out.get("result"))
    print("[AutoGen] verification:", out.get("verification"))


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--mission", default="coordinate multi-agent implementation")
    args = parser.parse_args()
    demo(args.mission)
