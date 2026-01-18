
from pathlib import Path

import pandas as pd
import sqlite3

# Path to the SQLite database file
DB_PATH = Path(__file__).parent / "stats-mag-2026.db"

# Number of .NET ticks between 0001-01-01 and 1970-01-01
DOTNET_TO_UNIX_TICKS = 621355968000000000

def convert_dates(df):
    for col in df.columns:
        if col.endswith("Time"):
            ticks = df[col].astype("Int64")

            # Convert .NET ticks → Unix nanoseconds
            unix_ns = (ticks - DOTNET_TO_UNIX_TICKS) * 100

            df[col] = pd.to_datetime(
                unix_ns,
                unit="ns",
                errors="coerce",
                utc=True,   # optional but recommended
            ).dt.floor("s")
        elif col.endswith("Duration"):
            ticks = df[col].astype("Int64")

            # Convert .NET ticks → nanoseconds
            ns = ticks * 100

            df[col] = pd.to_timedelta(
                ns,
                unit="ns",
                errors="coerce"
            )
    return df


def query_db(query: str, do_convert_dates=True) -> pd.DataFrame:
    """Execute a SQL query and return the results as a DataFrame."""
    with sqlite3.connect(DB_PATH) as conn:
        df = pd.read_sql_query(query, conn)
        if do_convert_dates:
            df = convert_dates(df)
        return df


def create_analytic_database(db_path: Path, setup_query: Path) -> Path:
    """Create an analytic database by copying the existing database."""
    import shutil

    analytic_db_path = db_path.parent / f"{db_path.stem}-analytic{db_path.suffix}"
    shutil.copy(db_path, analytic_db_path)

    with sqlite3.connect(analytic_db_path) as conn:
        setup_sql = setup_query.read_text()
        conn.executescript(setup_sql)

    return analytic_db_path
