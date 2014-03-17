//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "ViewController.h"
#import "KSClientSocket.h"

@interface ViewController ()

@property KSClientSocket * socket;

@end

@implementation ViewController

- (void)viewDidLoad
{
    [super viewDidLoad];
	// Do any additional setup after loading the view, typically from a nib.
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}


/*-------------------------------------------------------------------------------
 * Connect Button & Socket Event Handlers
 *--------------------------------------------------------------------------------*/

- (IBAction)connectButtonTouchUpInside:(UIButton *)sender
{
    if(sender.selected == NO)
    {
        sender.selected = YES;
        [self connect];
    }
    else
    {
        sender.selected = NO;
        [self disconnect];
    }
}

- (void)connect
{
    NSString * address = self.ipAddressTextField.text;
    uint port = [self.portTextFiled.text intValue];
    
    // selection of protocol
    switch (self.protocolSegmentControl.selectedSegmentIndex)
    {
        case 0:
            self.socket = [[KSClientSocket alloc] initVariableSizeSocket:address serverPort:port];
            break;
            
        case 1:
            self.socket = [[KSClientSocket alloc] initFixedSizeSocket:address serverPort:port size:[self.messageSizeTextField.text intValue]];
            break;
            
        default:
            self.socket = [[KSClientSocket alloc] initTerminatedMessageSocket:address serverPort:port terminator:self.terminationStringTextField.text];
            break;
    }
    
    [self.socket.connected addEventListener:self selector:@selector(connectedHandler:)];
    [self.socket.received addEventListener:self selector:@selector(receivedHandler:)];
    [self.socket.disconnected addEventListener:self selector:@selector(disconnectedHandler:)];
    [self.socket.error addEventListener:self selector:@selector(errorHandler:)];
    
    [self insertLog:@"start acceptting"];
    [self.socket start];
}

- (void)disconnect
{
    [self insertLog:@"stop acceptting"];
    
    [self.socket.connected removeEventListener:self selector:@selector(connectedHandler:)];
    [self.socket.received removeEventListener:self selector:@selector(receivedHandler:)];
    [self.socket.disconnected removeEventListener:self selector:@selector(disconnectedHandler:)];
    [self.socket.error removeEventListener:self selector:@selector(errorHandler:)];
    [self.socket close];
    self.socket = nil;
}


// socket event handlers
- (void)connectedHandler:(id)object
{
    [self insertLog:[NSString stringWithFormat:@"connected : %@:%@", self.ipAddressTextField.text, self.portTextFiled.text]];
}

- (void)receivedHandler:(NSString *)message
{
    [self insertLog:[NSString stringWithFormat:@"received : %@", message]];
}

- (void)disconnectedHandler:(id)object
{
    [self insertLog:[NSString stringWithFormat:@"disconnected"]];
}

- (void)errorHandler:(NSString *)message
{
    [self insertLog:[NSString stringWithFormat:@"error : %@", message]];
}


/*-------------------------------------------------------------------------------
 * Send Button
 *--------------------------------------------------------------------------------*/

- (IBAction)sendButtonTouchUpInside:(UIButton *)sender
{
    if(self.socket == nil)
    {
        return;
    }
    [self.socket send:_messageTextBox.text];
}


/*-------------------------------------------------------------------------------
 * SegmentValuesでプロトコルを選択した際のアニメーション
 *--------------------------------------------------------------------------------*/
- (IBAction)protocolSegmentValueChanged:(id)sender
{
    switch (_protocolSegmentControl.selectedSegmentIndex)
    {
        case 0:
            [self scrollProtocolSettingSliderViewTo:0];
            break;
            
        case 1:
            [self scrollProtocolSettingSliderViewTo:-320];
            break;
            
        case 2:
            [self scrollProtocolSettingSliderViewTo:-640];
            break;
            
        default:
            break;
    }
}

- (void)scrollProtocolSettingSliderViewTo:(int)y
{
    [UIView beginAnimations:nil context:nil];
    [UIView setAnimationCurve:UIViewAnimationCurveEaseInOut];
    _protocolSettingSliderView.frame = CGRectOffset(_protocolSettingSliderView.frame, y - _protocolSettingSliderView.frame.origin.x, 0);
    [UIView commitAnimations];
}


/*-------------------------------------------------------------------------------
 * Random Button
 *--------------------------------------------------------------------------------*/

- (IBAction)randomButtonTouchUpInside:(UIButton *)sender
{
    self.messageTextBox.text = [self randStringWithMaxLenght:10];
}

// 下の２メソッドはwebからのまるぱくり
- (NSString *)randStringWithMaxLenght:(NSInteger)max
{
    NSInteger length = [self randBetween:1 max:max];
    unichar letter[length];
    for (int i = 0; i < length; i++) {
        letter[i] = [self randBetween:65 max:90];
    }
    return [[NSString alloc] initWithCharacters:letter length:length];
}
- (NSInteger)randBetween:(NSInteger)min max:(NSInteger)max {
    return (random() % (max - min + 1)) + min;
}



/*-------------------------------------------------------------------------------
 * TextFiled関連
 *--------------------------------------------------------------------------------*/

- (IBAction)ipAddressTextFieldDidEndOnExit:(id)sender
{
    // do nothing
}


- (IBAction)portTextFieldDidEndOnExit:(id)sender
{
    // do nothing
}

- (IBAction)messageSizeTextFieldDidEndOnExit:(id)sender
{
    // do nothing
}

- (IBAction)terminationStringTextFiledDidEndOnExit:(id)sender
{
    // do nothing
}


/*-------------------------------------------------------------------------------
 * TextFieldがキーボードに重ならないようにアニメーション
 *--------------------------------------------------------------------------------*/
- (IBAction)messageTextFiledEditingDidBegin:(id)sender
{
    [UIView beginAnimations:nil context:nil];
    [UIView setAnimationCurve:UIViewAnimationCurveEaseInOut];
    _scrollView.frame = CGRectOffset(_scrollView.frame, 0, -190);
    [UIView commitAnimations];
}

- (IBAction)messageTextFieldDidEndOnExit:(id)sender
{
    [UIView beginAnimations:nil context:nil];
    [UIView setAnimationCurve:UIViewAnimationCurveEaseInOut];
    _scrollView.frame = CGRectOffset(_scrollView.frame, 0, 190);
    [UIView commitAnimations];
}


/*-------------------------------------------------------------------------------
 * Longging
 *--------------------------------------------------------------------------------*/

- (void)insertLog:(NSString *)message
{
    if([_logTextView.text isEqualToString:@""])
    {
        _logTextView.text = [_logTextView.text stringByAppendingString:message];
    }
    else
    {
        _logTextView.text = [_logTextView.text stringByAppendingFormat:@"\n%@", message];
    }
    
    NSRange range = NSMakeRange(_logTextView.text.length - 1, 1);
    [_logTextView scrollRangeToVisible:range];
}


@end
