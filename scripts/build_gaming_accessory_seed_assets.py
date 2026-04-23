#!/usr/bin/env python3
from __future__ import annotations

import argparse
import html
import json
import re
import shutil
import sys
import time
import unicodedata
from dataclasses import dataclass
from datetime import date
from pathlib import Path
from typing import Iterable
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode, urljoin, urlparse, parse_qsl, urlunparse
from urllib.request import Request, urlopen


USER_AGENT = (
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
    "AppleWebKit/537.36 (KHTML, like Gecko) "
    "Chrome/134.0.0.0 Safari/537.36"
)
REQUEST_HEADERS = {"User-Agent": USER_AGENT}
REPO_ROOT = Path(__file__).resolve().parents[1]
DATASET_PATH = REPO_ROOT / "DataAccess" / "Seeding" / "gaming_accessories_seed_data.json"
SAMPLE_DOC_PATH = REPO_ROOT / "docs" / "sample_products.md"
ASSET_DIR = REPO_ROOT / "Assets" / "GamingProducts"
SOURCE_IMAGE_DIR = REPO_ROOT / "Assets" / "_source_gaming_images"


@dataclass(frozen=True)
class CategoryConfig:
    source_key: str
    app_category: str
    source_url: str
    count: int
    sku_prefix: str
    image_ids: tuple[str, str, str]
    brand_priority: tuple[str, ...]
    name_priority: tuple[str, ...]
    spec_priority: tuple[str, ...]
    required_terms: tuple[str, ...] = ()
    exclude_terms: tuple[str, ...] = ()


CATEGORY_CONFIGS: tuple[CategoryConfig, ...] = (
    CategoryConfig(
        source_key="keyboard",
        app_category="Gaming Keyboard",
        source_url="https://phongvu.vn/c/ban-phim-gaming",
        count=22,
        sku_prefix="GKB",
        image_ids=("mm2cc_I3-iM", "ePGW9e_gcz8", "OaPksPcVp50"),
        brand_priority=("Logitech", "Razer", "SteelSeries", "Asus", "HyperX", "Corsair", "Aula", "Dareu"),
        name_priority=("G Pro X", "BlackWidow V4", "Apex Pro", "ROG Azoth", "Huntsman", "G515", "K70"),
        spec_priority=("kết nối", "layout", "kiểu bàn phím", "switch", "đèn", "màu sắc", "kích thước"),
        required_terms=("gaming", "blackwidow", "huntsman", "rog", "g515", "k70", "falchion", "azoth", "makr"),
        exclude_terms=("alto keys",),
    ),
    CategoryConfig(
        source_key="mouse",
        app_category="Gaming Mouse",
        source_url="https://phongvu.vn/c/chuot-gaming",
        count=22,
        sku_prefix="GMS",
        image_ids=("LaEOxuvzRnY", "KNefG4CVCnU", "Zah7GC6mNKg"),
        brand_priority=("Razer", "Logitech", "SteelSeries", "Asus", "HyperX", "Pulsar", "Dareu", "Fantech"),
        name_priority=("DeathAdder V3", "G502 X", "Superlight", "Viper V3", "Aerox", "Harpe Ace"),
        spec_priority=("kết nối", "kiểu cầm", "dpi", "cảm biến", "tên cảm biến", "số nút", "khối lượng", "màu sắc"),
        required_terms=("gaming", "deathadder", "g502", "superlight", "viper", "basilisk", "keris", "harpe", "aerox"),
    ),
    CategoryConfig(
        source_key="headset",
        app_category="Gaming Headset",
        source_url="https://phongvu.vn/c/tai-nghe-gaming",
        count=22,
        sku_prefix="GHS",
        image_ids=("ZPtfDDp_SlI", "u4YHPDVolT4", "FhMSavrbn1M"),
        brand_priority=("SteelSeries", "HyperX", "Logitech", "Razer", "Asus", "Corsair", "Sony"),
        name_priority=("Arctis Nova 7", "Cloud III", "BlackShark", "G Pro X", "ROG Delta", "Barracuda"),
        spec_priority=("kết nối", "kiểu tai nghe", "driver", "micro", "độ nhạy", "tần số", "màu sắc"),
        exclude_terms=("ier-",),
    ),
    CategoryConfig(
        source_key="mousepad",
        app_category="Mousepad",
        source_url="https://phongvu.vn/c/lot-chuot",
        count=22,
        sku_prefix="MPD",
        image_ids=("O53iMVN35VI", "ZxQzBGGllMQ", "yfQJ0T2RVyA"),
        brand_priority=("SteelSeries", "Razer", "Logitech", "HyperX", "Corsair", "Pulsar", "Dareu"),
        name_priority=("QcK", "Gigantus", "G640", "Firefly", "Fury", "MM300"),
        spec_priority=("kích thước", "chất liệu", "độ dày", "màu sắc", "tính năng", "bề mặt"),
        required_terms=("chuột", "mouse", "qck", "gigantus", "g640", "firefly", "sphex"),
    ),
    CategoryConfig(
        source_key="webcam",
        app_category="Streaming Gear",
        source_url="https://phongvu.vn/c/webcam",
        count=11,
        sku_prefix="STG",
        image_ids=("Qruwi3Ur3Ak", "jIrAUWcHOcI", "OKLqGsCT8qs"),
        brand_priority=("Logitech", "Elgato", "Avermedia", "Rapoo", "Microsoft"),
        name_priority=("C920", "C922", "Brio", "Facecam", "StreamCam", "MX Brio"),
        spec_priority=("độ phân giải", "fps", "kết nối", "micro", "màu sắc", "tính năng", "góc nhìn"),
        required_terms=("webcam", "facecam", "brio", "c920", "c922", "streamcam"),
    ),
    CategoryConfig(
        source_key="microphone",
        app_category="Streaming Gear",
        source_url="https://phongvu.vn/c/microphone",
        count=11,
        sku_prefix="STM",
        image_ids=("UUPpu2sYV6E", "sY2fRBkcG1U", "KNYcGEgwZmg"),
        brand_priority=("HyperX", "Razer", "Elgato", "Audio-Technica", "Saramonic", "Maono"),
        name_priority=("QuadCast", "Seiren", "Wave", "SoloCast", "AT2020"),
        spec_priority=("kết nối", "hướng thu", "tần số", "độ nhạy", "tính năng", "màu sắc", "phụ kiện"),
        required_terms=("micro", "quadcast", "seiren", "wave", "solocast", "at2020"),
    ),
)

HARD_SKIP_SPEC_PATTERNS = (
    "đơn vị tính",
    "tỉ lệ quy đổi",
    "cân nặng đóng gói",
    "chiều dài đóng gói",
    "chiều rộng đóng gói",
    "chiều cao đóng gói",
    "chiều dài",
    "chiều rộng",
    "chiều cao",
    "barcode",
    "part number",
    "series",
    "loại hàng",
    "nhu cầu",
)

SOFT_SKIP_SPEC_PATTERNS = (
    "thương hiệu",
    "bảo hành",
    "part number",
    "series",
    "tên",
)


def normalize_text(value: str) -> str:
    normalized = unicodedata.normalize("NFD", value)
    stripped = "".join(char for char in normalized if unicodedata.category(char) != "Mn")
    return re.sub(r"\s+", " ", stripped).strip().lower()


def fetch_text(url: str, *, binary: bool = False, retries: int = 3) -> str | bytes:
    last_error: Exception | None = None
    for attempt in range(1, retries + 1):
        request = Request(url, headers=REQUEST_HEADERS)
        try:
            with urlopen(request, timeout=45) as response:
                payload = response.read()
                return payload if binary else payload.decode("utf-8", "ignore")
        except (HTTPError, URLError, TimeoutError) as error:
            last_error = error
            if attempt == retries:
                break
            time.sleep(1.25 * attempt)

    assert last_error is not None
    raise last_error


def extract_next_data(document: str) -> dict:
    match = re.search(r'<script id="__NEXT_DATA__" type="application/json">(.*?)</script>', document, re.S)
    if not match:
        raise RuntimeError("Could not locate __NEXT_DATA__ payload.")

    return json.loads(match.group(1))


def build_category_rank(name: str, config: CategoryConfig) -> tuple[int, int, str]:
    normalized_name = normalize_text(name)

    def priority_index(values: Iterable[str]) -> int:
        for index, value in enumerate(values):
            if normalize_text(value) in normalized_name:
                return index
        return 999

    name_rank = priority_index(config.name_priority)
    brand_rank = priority_index(config.brand_priority)
    return name_rank, brand_rank, normalized_name


def pick_category_products(config: CategoryConfig) -> list[dict]:
    unique_products: dict[str, dict] = {}

    for page in range(1, 7):
        page_url = with_query_parameter(config.source_url, "page", str(page))
        document = fetch_text(page_url)
        payload = extract_next_data(document)
        products = payload["props"]["pageProps"].get("serverProducts") or []
        if not products:
            break

        for product in products:
            pathname = product.get("link", {}).get("as", {}).get("pathname")
            latest_price = product.get("price", {}).get("latestPrice") or 0
            if not pathname or latest_price <= 0:
                continue

            normalized_name = normalize_text(product.get("name", ""))
            if config.required_terms and not any(
                normalize_text(term) in normalized_name for term in config.required_terms
            ):
                continue
            if any(normalize_text(term) in normalized_name for term in config.exclude_terms):
                continue

            unique_products[pathname] = product

        if len(unique_products) >= config.count:
            break

    ranked = sorted(
        unique_products.values(),
        key=lambda product: build_category_rank(product.get("name", ""), config),
    )
    return ranked[: config.count]


def with_query_parameter(url: str, key: str, value: str) -> str:
    parsed = urlparse(url)
    query = dict(parse_qsl(parsed.query, keep_blank_values=True))
    query[key] = value
    return urlunparse(parsed._replace(query=urlencode(query)))


def strip_html(value: str) -> str:
    if not value:
        return ""

    text = re.sub(r"<[^>]+>", " ", value)
    text = html.unescape(text)
    return re.sub(r"\s+", " ", text).strip()


def build_description(product_name: str, meta_description: str, html_description: str) -> str:
    candidate = strip_html(html_description)
    normalized_candidate = normalize_text(candidate)
    if not candidate or normalized_candidate == "dang cap nhat":
        candidate = ""

    if not candidate:
        candidate = strip_html(meta_description)

    normalized_candidate = normalize_text(candidate)
    if not candidate or normalized_candidate == "dang cap nhat" or len(candidate) < 24:
        return f"{product_name} is a current Vietnam-market gaming accessory sourced from Phong Vu with seeded retail pricing in VND."

    sentences = [segment.strip(" -") for segment in re.split(r"(?<=[.!?])\s+", candidate) if segment.strip()]

    for sentence in sentences:
        normalized = normalize_text(sentence)
        if not normalized.startswith("mua ") and len(sentence) >= 60:
            return sentence[:220].rstrip()

    return candidate[:220].rstrip()


def build_specs(config: CategoryConfig, attributes: list[dict]) -> list[str]:
    candidates: list[tuple[str, str, str]] = []
    fallback_candidates: list[tuple[str, str, str]] = []

    for attribute in attributes:
        raw_name = str(attribute.get("name") or "").strip()
        values = attribute.get("values") or []
        if not raw_name or not values:
            continue

        joined_value = ", ".join(strip_html(str(value).strip()) for value in values if str(value).strip())
        if not joined_value:
            continue

        normalized_name = normalize_text(raw_name)
        line = f"{raw_name}: {joined_value}"
        if any(normalize_text(pattern) in normalized_name for pattern in HARD_SKIP_SPEC_PATTERNS):
            continue

        fallback_candidates.append((raw_name, joined_value, line))

        if any(normalize_text(pattern) in normalized_name for pattern in SOFT_SKIP_SPEC_PATTERNS):
            continue

        candidates.append((raw_name, joined_value, line))

    specs: list[str] = []
    used_lines: set[str] = set()

    for keyword in config.spec_priority:
        normalized_keyword = normalize_text(keyword)
        for name, _, line in candidates:
            if line in used_lines:
                continue
            if normalized_keyword in normalize_text(name):
                specs.append(line)
                used_lines.add(line)
                break

    for _, _, line in candidates:
        if line in used_lines:
            continue
        specs.append(line)
        used_lines.add(line)
        if len(specs) == 5:
            return specs

    for _, _, line in fallback_candidates:
        if line in used_lines:
            continue
        specs.append(line)
        used_lines.add(line)
        if len(specs) == 5:
            return specs

    return specs[:5]


def fetch_product_record(config: CategoryConfig, summary: dict, running_index: int) -> dict:
    pathname = summary["link"]["as"]["pathname"]
    product_url = urljoin("https://phongvu.vn", pathname)
    document = fetch_text(product_url)
    payload = extract_next_data(document)
    product_root = payload["props"]["pageProps"]["serverProduct"]["product"]
    product_info = product_root["productInfo"]
    product_detail = product_root.get("productDetail") or {}
    detail_attributes = product_detail.get("attributes") or []

    brand = (
        summary.get("brand", {}).get("name")
        or product_info.get("brand", {}).get("name")
        or product_info.get("manufacturer")
        or ""
    ).strip()

    description = build_description(
        product_info.get("name", summary.get("name", "")),
        product_detail.get("metaDescription") or "",
        product_detail.get("description") or "",
    )

    specs = build_specs(config, detail_attributes)

    return {
        "sku": f"{config.sku_prefix}-{running_index:03d}",
        "name": product_info.get("name", summary.get("name", "")).strip(),
        "brand": brand,
        "category": config.app_category,
        "sourceType": config.source_key,
        "shortDescription": description,
        "priceVnd": int(summary["price"]["latestPrice"]),
        "sourceRetailer": "Phong Vu",
        "sourceUrl": product_url,
        "sourceCheckedOn": date.today().isoformat(),
        "specs": specs,
        "imageSet": config.source_key,
    }


def build_dataset() -> list[dict]:
    dataset: list[dict] = []
    running_index = 1

    for config in CATEGORY_CONFIGS:
        summaries = pick_category_products(config)
        if len(summaries) < config.count:
            raise RuntimeError(f"Only found {len(summaries)} products for {config.source_key}.")

        for summary in summaries:
            dataset.append(fetch_product_record(config, summary, running_index))
            running_index += 1
            time.sleep(0.15)

    return dataset


def validate_dataset(dataset: list[dict]) -> None:
    category_counts: dict[str, int] = {}
    for item in dataset:
        category_counts[item["category"]] = category_counts.get(item["category"], 0) + 1

    invalid = {
        category: count
        for category, count in category_counts.items()
        if count < 22
    }
    expected_categories = {
        "Gaming Keyboard",
        "Gaming Mouse",
        "Gaming Headset",
        "Mousepad",
        "Streaming Gear",
    }

    missing_categories = expected_categories.difference(category_counts)
    if invalid or missing_categories:
        raise RuntimeError(
            f"Dataset validation failed. Counts={category_counts}, invalid={invalid}, missing={sorted(missing_categories)}"
        )


def write_dataset(dataset: list[dict]) -> None:
    DATASET_PATH.parent.mkdir(parents=True, exist_ok=True)
    DATASET_PATH.write_text(json.dumps(dataset, ensure_ascii=False, indent=2), encoding="utf-8")


def write_sample_doc(dataset: list[dict]) -> None:
    lines = [
        "# Sample Products",
        "",
        "Real Vietnamese-market gaming accessory products collected from Phong Vu and used by the WinUI 3 POS seed dataset.",
        "",
        "| Product | Category | Price VND |",
        "| --- | --- | ---: |",
    ]

    for item in dataset:
        lines.append(f"| {item['name']} | {item['category']} | {item['priceVnd']:,} |")

    lines.extend(
        [
            "",
            "## Source",
            "",
            "- Retail pricing source: `https://phongvu.vn`",
            "- Price check date: "
            + ", ".join(sorted({item["sourceCheckedOn"] for item in dataset})),
            "- Public image source used for packaged product galleries: Unsplash",
        ]
    )

    SAMPLE_DOC_PATH.parent.mkdir(parents=True, exist_ok=True)
    SAMPLE_DOC_PATH.write_text("\n".join(lines) + "\n", encoding="utf-8")


def ensure_base_images() -> None:
    SOURCE_IMAGE_DIR.mkdir(parents=True, exist_ok=True)

    for config in CATEGORY_CONFIGS:
        for index, photo_id in enumerate(config.image_ids, start=1):
            target_path = SOURCE_IMAGE_DIR / f"{config.source_key}_{index}.jpg"
            if target_path.exists():
                continue

            image_url = f"https://unsplash.com/photos/{photo_id}/download?force=true&w=720&q=80"
            payload = fetch_text(image_url, binary=True)
            target_path.write_bytes(payload)


def build_product_images(dataset: list[dict]) -> None:
    ensure_base_images()
    ASSET_DIR.mkdir(parents=True, exist_ok=True)

    for existing_file in ASSET_DIR.glob("*.jpg"):
        existing_file.unlink()

    for product_id, item in enumerate(dataset, start=1):
        image_set = item["imageSet"]
        for image_number in range(1, 4):
            source_path = SOURCE_IMAGE_DIR / f"{image_set}_{image_number}.jpg"
            target_path = ASSET_DIR / f"{product_id}_{image_number}.jpg"
            shutil.copyfile(source_path, target_path)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Build the gaming accessories seed dataset, sample document, and packaged images."
    )
    parser.add_argument("--dataset-only", action="store_true", help="Refresh the JSON dataset and markdown sample file.")
    parser.add_argument("--images-only", action="store_true", help="Refresh packaged images from the existing dataset JSON.")
    return parser.parse_args()


def main() -> int:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")

    args = parse_args()
    if args.dataset_only and args.images_only:
        raise SystemExit("Use either --dataset-only or --images-only, not both.")

    dataset: list[dict]
    if args.images_only:
        dataset = json.loads(DATASET_PATH.read_text(encoding="utf-8"))
    else:
        dataset = build_dataset()
        validate_dataset(dataset)
        write_dataset(dataset)
        write_sample_doc(dataset)

    if not args.dataset_only:
        build_product_images(dataset)

    print(f"Products: {len(dataset)}")
    print(f"Dataset: {DATASET_PATH}")
    print(f"Sample doc: {SAMPLE_DOC_PATH}")
    print(f"Images: {ASSET_DIR}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
