# Manual package builder

Run `python build-manual.py --config manual-config.json --output artifacts` with Python 3. Downloads are pinned by hash; all selected entries are verified. The package excludes game files and private data. Seven plugins and original notices are retained. `manual-verification.json` records the resulting artifact hash. No role code is changed by this packaging.
