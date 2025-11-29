import { Box, Fade } from '@mui/material';
import { ReactNode } from 'react';

interface FadeInProps {
  /** Content to fade in */
  children: ReactNode;
  /** Delay before animation starts (in milliseconds) */
  delay?: number;
  /** Animation duration (in milliseconds) */
  timeout?: number;
}

/**
 * Wrapper component that fades in its children.
 * Useful for staggered animations on page load.
 * 
 * @example
 * <FadeIn delay={100}>
 *   <MyComponent />
 * </FadeIn>
 */
const FadeIn = ({ children, delay = 0, timeout = 500 }: FadeInProps) => {
  return (
    <Fade
      in
      timeout={timeout}
      style={{ transitionDelay: `${delay}ms` }}
    >
      <Box>{children}</Box>
    </Fade>
  );
};

export default FadeIn;
