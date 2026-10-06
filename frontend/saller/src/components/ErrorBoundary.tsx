import { Component, type ErrorInfo, type ReactNode } from 'react'

type Props = { children: ReactNode }

type State = { error: Error | null }

export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null }

  static getDerivedStateFromError(error: Error): State {
    return { error }
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    console.error('Render xətası:', error, info.componentStack)
  }

  render() {
    if (this.state.error) {
      return (
        <div className="flex min-h-screen flex-col items-center justify-center bg-slate-50 px-4 py-16 text-center">
          <p className="text-lg font-semibold text-slate-900">Nəsə səhv getdi</p>
          <p className="mt-2 max-w-md text-sm text-slate-600">
            Səhifəni yeniləyin. Problem davam edərsə, dəstəklə əlaqə saxlayın.
          </p>
          <button
            type="button"
            onClick={() => globalThis.location.reload()}
            className="mt-6 rounded-lg bg-[#006437] px-4 py-2.5 text-sm font-semibold text-white hover:bg-[#005029] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
          >
            Yenilə
          </button>
        </div>
      )
    }
    return this.props.children
  }
}
