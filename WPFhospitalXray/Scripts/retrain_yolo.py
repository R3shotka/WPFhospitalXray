from pathlib import Path
import argparse
import json
import math
import random
import shutil
from typing import List, Optional, Tuple

import yaml
from ultralytics import YOLO


IMAGE_EXTENSIONS = {".jpg", ".jpeg", ".png", ".bmp", ".webp"}
SEED = 42
IMGSZ = 640
BATCH = 16

ImageLabelPair = Tuple[Path, Path]


def parse_args():
    parser = argparse.ArgumentParser()

    parser.add_argument("--old-data", required=True)
    parser.add_argument("--old-model", required=True)
    parser.add_argument("--new-data", required=True)
    parser.add_argument("--output-root", required=True)
    parser.add_argument("--experiment", default="E5_mix_67old_33new_full")
    parser.add_argument("--device", default="0")
    parser.add_argument("--epochs", type=int, default=80)
    parser.add_argument("--dry-run", default="false")

    return parser.parse_args()


def is_dry_run(value: str) -> bool:
    return str(value).strip().lower() in ["true", "1", "yes"]


def load_yaml(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as file:
        return yaml.safe_load(file)


def resolve_dataset_base(data_yaml: Path, data: dict) -> Path:
    raw_path = data.get("path")

    if raw_path is None:
        return data_yaml.parent.resolve()

    base_path = Path(str(raw_path))

    if base_path.is_absolute():
        return base_path.resolve()

    return (data_yaml.parent / base_path).resolve()


def resolve_split_dir(data_yaml: Path, split_name: str) -> Optional[Path]:
    data = load_yaml(data_yaml)
    base_path = resolve_dataset_base(data_yaml, data)
    split_value = data.get(split_name)

    if split_value is None:
        return None

    if isinstance(split_value, list):
        raise ValueError(f"List-format split is not supported: {split_name}")

    split_path = Path(str(split_value))

    if split_path.is_absolute():
        return split_path.resolve()

    return (base_path / split_path).resolve()


def find_images(images_dir: Optional[Path]) -> List[Path]:
    if images_dir is None or not images_dir.exists():
        return []

    return sorted(
        path
        for path in images_dir.rglob("*")
        if path.is_file() and path.suffix.lower() in IMAGE_EXTENSIONS
    )


def infer_label_path(image_path: Path) -> Path:
    parts = list(image_path.parts)

    if "images" in parts:
        index = parts.index("images")
        parts[index] = "labels"
        return Path(*parts).with_suffix(".txt")

    return image_path.parent.parent / "labels" / image_path.parent.name / f"{image_path.stem}.txt"


def is_number(value: str) -> bool:
    try:
        number = float(value)
        return math.isfinite(number)
    except ValueError:
        return False


def validate_label(label_path: Path) -> List[str]:
    errors = []

    if not label_path.exists():
        errors.append("label file is missing")
        return errors

    lines = label_path.read_text(encoding="utf-8", errors="ignore").splitlines()

    # Empty labels are valid for negative/background samples.
    if not lines:
        return errors

    for line_number, line in enumerate(lines, start=1):
        stripped = line.strip()

        if not stripped:
            continue

        parts = stripped.split()

        if len(parts) != 5 and (len(parts) < 7 or (len(parts) - 1) % 2 != 0):
            errors.append(
                f"line {line_number}: expected bbox format with 5 values "
                f"or polygon format with class_id and coordinate pairs, got {len(parts)}"
            )
            continue

        if not all(is_number(part) for part in parts):
            errors.append(f"line {line_number}: non-numeric value found")
            continue

        class_id = parts[0]

        if class_id != "0":
            errors.append(f"line {line_number}: class_id must be 0")

        if len(parts) == 5:
            x_center = float(parts[1])
            y_center = float(parts[2])
            width = float(parts[3])
            height = float(parts[4])

            if not 0 <= x_center <= 1:
                errors.append(f"line {line_number}: x_center is outside 0..1")

            if not 0 <= y_center <= 1:
                errors.append(f"line {line_number}: y_center is outside 0..1")

            if not 0 < width <= 1:
                errors.append(f"line {line_number}: width is outside 0..1")

            if not 0 < height <= 1:
                errors.append(f"line {line_number}: height is outside 0..1")
        else:
            coordinates = [float(value) for value in parts[1:]]
            xs = coordinates[0::2]
            ys = coordinates[1::2]

            for point_index, x_value in enumerate(xs, start=1):
                if not 0 <= x_value <= 1:
                    errors.append(f"line {line_number}: polygon x{point_index} is outside 0..1")

            for point_index, y_value in enumerate(ys, start=1):
                if not 0 <= y_value <= 1:
                    errors.append(f"line {line_number}: polygon y{point_index} is outside 0..1")

            width = max(xs) - min(xs)
            height = max(ys) - min(ys)

            if width <= 0:
                errors.append(f"line {line_number}: polygon width must be greater than 0")

            if height <= 0:
                errors.append(f"line {line_number}: polygon height must be greater than 0")

    return errors


def collect_pairs(data_yaml: Path, split_name: str) -> List[ImageLabelPair]:
    images_dir = resolve_split_dir(data_yaml, split_name)
    images = find_images(images_dir)
    pairs = []
    problems = []

    for image_path in images:
        label_path = infer_label_path(image_path)
        errors = validate_label(label_path)

        if errors:
            problems.append((image_path, label_path, errors))
        else:
            pairs.append((image_path, label_path))

    if problems:
        problem_report = data_yaml.parent / f"label_problems_{split_name}.txt"

        with problem_report.open("w", encoding="utf-8") as file:
            for image_path, label_path, errors in problems:
                file.write(f"{image_path}\n")
                file.write(f"{label_path}\n")

                for error in errors:
                    file.write(f"  - {error}\n")

                file.write("\n")

        raise RuntimeError(
            f"Invalid labels were found in split '{split_name}'. "
            f"Report: {problem_report}"
        )

    return pairs


def load_dataset_pairs(data_yaml: Path):
    return {
        "train": collect_pairs(data_yaml, "train"),
        "val": collect_pairs(data_yaml, "val"),
        "test": collect_pairs(data_yaml, "test"),
    }


def reset_dir(path: Path):
    if path.exists():
        shutil.rmtree(path)

    path.mkdir(parents=True, exist_ok=True)


def make_yolo_dirs(dataset_dir: Path):
    for split in ["train", "val", "test"]:
        (dataset_dir / "images" / split).mkdir(parents=True, exist_ok=True)
        (dataset_dir / "labels" / split).mkdir(parents=True, exist_ok=True)


def normalize_and_copy_label(src_label: Path, dst_label: Path):
    lines_out = []

    if src_label.exists():
        lines = src_label.read_text(encoding="utf-8", errors="ignore").splitlines()

        for line in lines:
            stripped = line.strip()

            if not stripped:
                continue

            parts = stripped.split()

            if len(parts) != 5 and (len(parts) < 7 or (len(parts) - 1) % 2 != 0):
                continue

            if not all(is_number(part) for part in parts):
                continue

            if len(parts) == 5:
                _, x_center, y_center, width, height = parts
            else:
                coordinates = [float(value) for value in parts[1:]]
                xs = coordinates[0::2]
                ys = coordinates[1::2]

                min_x = min(xs)
                max_x = max(xs)
                min_y = min(ys)
                max_y = max(ys)

                x_center = (min_x + max_x) / 2
                y_center = (min_y + max_y) / 2
                width = max_x - min_x
                height = max_y - min_y

            lines_out.append(
                f"0 {float(x_center):.6f} {float(y_center):.6f} "
                f"{float(width):.6f} {float(height):.6f}"
            )

    dst_label.write_text("\n".join(lines_out) + ("\n" if lines_out else ""), encoding="utf-8")


def copy_pairs_to_split(pairs: List[ImageLabelPair], dataset_dir: Path, split: str, prefix: str):
    for image_path, label_path in pairs:
        new_name = f"{prefix}_{image_path.name}"
        new_stem = Path(new_name).stem

        dst_image = dataset_dir / "images" / split / new_name
        dst_label = dataset_dir / "labels" / split / f"{new_stem}.txt"

        shutil.copy2(image_path, dst_image)
        normalize_and_copy_label(label_path, dst_label)


def sample_pairs(pairs: List[ImageLabelPair], count: int, seed: int) -> List[ImageLabelPair]:
    if count > len(pairs):
        print(f"[WARN] Requested {count} old samples, but only {len(pairs)} are available.")
        count = len(pairs)

    rng = random.Random(seed)
    return rng.sample(pairs, count)


def write_data_yaml(dataset_dir: Path) -> Path:
    content = f"""path: {dataset_dir.as_posix()}

train: images/train
val: images/val
test: images/test

nc: 1
names: ['Fracture']
"""

    yaml_path = dataset_dir / "data.yaml"
    yaml_path.write_text(content, encoding="utf-8")
    return yaml_path


def create_single_test_dataset(pairs: List[ImageLabelPair], output_dir: Path, prefix: str) -> Path:
    reset_dir(output_dir)
    make_yolo_dirs(output_dir)

    copy_pairs_to_split(pairs, output_dir, "test", prefix)
    copy_pairs_to_split(pairs, output_dir, "val", prefix)
    copy_pairs_to_split(pairs, output_dir, "train", prefix)

    return write_data_yaml(output_dir)


def create_mix_67old_33new(output_dir: Path, old_pairs, new_pairs) -> Path:
    reset_dir(output_dir)
    make_yolo_dirs(output_dir)

    new_train = new_pairs["train"]
    new_val = new_pairs["val"]

    if not new_train:
        raise RuntimeError("The new dataset has no train samples.")

    if not new_val:
        raise RuntimeError("The new dataset has no validation samples.")

    old_train = sample_pairs(old_pairs["train"], len(new_train) * 2, SEED + 10)
    old_val = sample_pairs(old_pairs["val"], len(new_val) * 2, SEED + 20)

    if not old_train:
        raise RuntimeError("The old dataset has no train samples.")

    if not old_val:
        raise RuntimeError("The old dataset has no validation samples.")

    copy_pairs_to_split(new_train, output_dir, "train", "new")
    copy_pairs_to_split(old_train, output_dir, "train", "old")

    copy_pairs_to_split(new_val, output_dir, "val", "new")
    copy_pairs_to_split(old_val, output_dir, "val", "old")

    copy_pairs_to_split(new_pairs["test"], output_dir, "test", "new")
    copy_pairs_to_split(old_pairs["test"], output_dir, "test", "old")

    return write_data_yaml(output_dir)


def extract_metrics(metrics):
    box = metrics.box

    return {
        "precision": float(box.mp),
        "recall": float(box.mr),
        "map50": float(box.map50),
        "map5095": float(box.map),
    }


def evaluate_model(model_pt: Path, data_yaml: Path, eval_name: str, output_root: Path, device: str):
    model = YOLO(str(model_pt))

    metrics = model.val(
        data=str(data_yaml),
        split="test",
        imgsz=IMGSZ,
        batch=BATCH,
        device=device,
        project=str(output_root / "eval"),
        name=eval_name,
        exist_ok=True,
        plots=True,
    )

    return extract_metrics(metrics)


def export_onnx(model_pt: Path, final_onnx: Path) -> Path:
    model = YOLO(str(model_pt))

    exported_path = Path(
        model.export(
            format="onnx",
            imgsz=IMGSZ,
            simplify=True,
            opset=12,
        )
    )

    if exported_path.resolve() != final_onnx.resolve():
        if final_onnx.exists():
            final_onnx.unlink()

        shutil.move(str(exported_path), final_onnx)

    return final_onnx


def main():
    args = parse_args()

    old_data_yaml = Path(args.old_data)
    old_model_pt = Path(args.old_model)
    new_data_yaml = Path(args.new_data)
    output_root = Path(args.output_root)
    dry_run = is_dry_run(args.dry_run)

    if args.experiment != "E5_mix_67old_33new_full":
        raise ValueError("This script supports only E5_mix_67old_33new_full.")

    if not old_data_yaml.exists():
        raise FileNotFoundError(f"Old data.yaml was not found: {old_data_yaml}")

    if not old_model_pt.exists() or old_model_pt.suffix.lower() != ".pt":
        raise FileNotFoundError(f"Active .pt model was not found: {old_model_pt}")

    if not new_data_yaml.exists():
        raise FileNotFoundError(f"New data.yaml was not found: {new_data_yaml}")

    output_root.mkdir(parents=True, exist_ok=True)

    print("Loading old and new datasets...")
    old_pairs = load_dataset_pairs(old_data_yaml)
    new_pairs = load_dataset_pairs(new_data_yaml)

    if not old_pairs["test"]:
        print("[WARN] The old dataset has no test split. Validation split will be used as old_test.")
        old_pairs["test"] = old_pairs["val"]

    if not new_pairs["test"]:
        print("[WARN] The new dataset has no test split. Validation split will be used as new_test.")
        new_pairs["test"] = new_pairs["val"]

    mixed_dataset_dir = output_root / "datasets" / "mix_67old_33new"
    mixed_data_yaml = create_mix_67old_33new(mixed_dataset_dir, old_pairs, new_pairs)

    eval_root = output_root / "eval_datasets"

    eval_new_yaml = create_single_test_dataset(
        pairs=new_pairs["test"],
        output_dir=eval_root / "eval_new_test",
        prefix="new",
    )

    eval_old_yaml = create_single_test_dataset(
        pairs=old_pairs["test"],
        output_dir=eval_root / "eval_old_test",
        prefix="old",
    )

    eval_combined_yaml = create_single_test_dataset(
        pairs=new_pairs["test"] + old_pairs["test"],
        output_dir=eval_root / "eval_combined_test",
        prefix="combined",
    )

    epochs = 1 if dry_run else args.epochs
    patience = 1 if dry_run else 30

    train_project = output_root / "train"
    train_name = args.experiment
    train_run_dir = train_project / train_name

    if train_run_dir.exists():
        shutil.rmtree(train_run_dir)

    print(f"Starting training: {args.experiment}")
    print(f"Epochs: {epochs}")
    print(f"Dry run: {dry_run}")

    model = YOLO(str(old_model_pt))

    model.train(
        data=str(mixed_data_yaml),
        epochs=epochs,
        imgsz=IMGSZ,
        batch=BATCH,
        device=args.device,
        optimizer="AdamW",
        lr0=0.0001,
        lrf=0.01,
        cos_lr=True,
        patience=patience,
        seed=SEED,
        deterministic=True,
        hsv_h=0.0,
        hsv_s=0.0,
        hsv_v=0.10,
        degrees=5.0,
        translate=0.05,
        scale=0.20,
        fliplr=0.5,
        flipud=0.0,
        mosaic=0.2,
        mixup=0.0,
        copy_paste=0.0,
        close_mosaic=10,
        project=str(train_project),
        name=train_name,
        exist_ok=True,
        plots=True,
        val=True,
    )

    best_pt = train_run_dir / "weights" / "best.pt"

    if not best_pt.exists():
        raise FileNotFoundError(f"best.pt was not found after training: {best_pt}")

    metrics = {
        "new_test": evaluate_model(best_pt, eval_new_yaml, f"{args.experiment}_new_test", output_root, args.device),
        "old_test": evaluate_model(best_pt, eval_old_yaml, f"{args.experiment}_old_test", output_root, args.device),
        "combined_test": evaluate_model(best_pt, eval_combined_yaml, f"{args.experiment}_combined_test", output_root, args.device),
    }

    metrics_path = output_root / "training_metrics.json"
    metrics_path.write_text(json.dumps(metrics, ensure_ascii=False, indent=2), encoding="utf-8")

    if dry_run:
        dry_run_result = {
            "dryRun": True,
            "message": "Dry run completed. Final model export and registration were skipped.",
            "metrics": metrics,
        }

        dry_run_path = output_root / "training_result_dryrun.json"
        dry_run_path.write_text(json.dumps(dry_run_result, ensure_ascii=False, indent=2), encoding="utf-8")

        print("Dry run completed. Final model export skipped.")
        print(f"Dry-run result: {dry_run_path}")
        return

    version = f"best_E5_{output_root.name.replace('run_', '')}"
    models_dir = output_root / "models"
    models_dir.mkdir(parents=True, exist_ok=True)

    final_pt = models_dir / f"{version}.pt"
    shutil.copy2(best_pt, final_pt)

    final_onnx = models_dir / f"{version}.onnx"
    export_onnx(final_pt, final_onnx)

    combined_metrics = metrics["combined_test"]

    result = {
        "modelName": "YOLOv8 fracture detector",
        "version": version,
        "onnxPath": str(final_onnx),
        "ptPath": str(final_pt),
        "precision": combined_metrics["precision"],
        "recall": combined_metrics["recall"],
        "map50": combined_metrics["map50"],
        "map5095": combined_metrics["map5095"],
        "metrics": metrics,
    }

    result_path = output_root / "training_result.json"
    result_path.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")

    print("Training completed successfully.")
    print(f"Result file: {result_path}")
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
