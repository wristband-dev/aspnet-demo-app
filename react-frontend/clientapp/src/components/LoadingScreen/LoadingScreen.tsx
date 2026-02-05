import styles from "./LoadingScreen.module.css";

export function LoadingScreen() {
    return (
      <div className={styles.fullScreen}>
          <p className={styles.centeredText}>Securing...</p>
      </div>
    );
}
